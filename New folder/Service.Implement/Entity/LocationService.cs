namespace Service.Implement.Entity
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Transactions;
    using System.Collections.Generic;
    using Core.ObjectModels.Entities;
    using Core.ObjectService.Repositories;
    using Core.ApplicationService.Business.EntityService;
    using Core.ApplicationService.Business.LogService;
    using Core.ObjectModels.Entities.EnumType;
    using System.Threading.Tasks;
    using System.IO;
    using Firebase.Storage;

    public class LocationService : _BaseService<Location>, ILocationService
    {
        private readonly IRepository<Photo> _photoRepository;
        private readonly IRepository<LocationSuggestion> _locationSuggestionRepository;
        private readonly IRepository<LocationPhoto> _locationPhotoRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly IRepository<Plan> _planRepository;
        private readonly IRepository<PlanLocation> _planLocationRepository;
        private readonly IRepository<BusinessHour> _businessHourRepository;
        private readonly IRepository<Creator> _creatorRepository;
        private readonly IRepository<Group> _grouprRepository;
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Report> _reportRepository;
        private readonly IRepository<ChangeRequest> _changeRequestRepository;

        public LocationService(ILoggingService loggingService, IUnitOfWork unitOfWork) : base(loggingService, unitOfWork)
        {
            _tagRepository = unitOfWork.GetRepository<Tag>();
            _photoRepository = unitOfWork.GetRepository<Photo>();
            _locationPhotoRepository = unitOfWork.GetRepository<LocationPhoto>();
            _businessHourRepository = unitOfWork.GetRepository<BusinessHour>();
            _locationSuggestionRepository = unitOfWork.GetRepository<LocationSuggestion>();
            _planRepository = unitOfWork.GetRepository<Plan>();
            _planLocationRepository = unitOfWork.GetRepository<PlanLocation>();
            _reviewRepository = unitOfWork.GetRepository<Review>();
            _creatorRepository = unitOfWork.GetRepository<Creator>();
            _grouprRepository = unitOfWork.GetRepository<Group>();
            _changeRequestRepository = unitOfWork.GetRepository<ChangeRequest>();
            _reportRepository = unitOfWork.GetRepository<Report>();
        }

        public IEnumerable<Report> GetAllReport()
        {
            return _reportRepository.GetAllAsQueryable(_ => _.User, _ => _.Review.Creator, _ => _.Review.Photos, _ => _.Review.Location).ToList();
        }

        public IEnumerable<ChangeRequest> GetAllChangeRequest()
        {
            return _changeRequestRepository.GetAllAsQueryable(_ => _.User).ToList();
        }


        public bool AcceptStatusLocationSuggestion(int suggestionId)
        {
            try
            {
                LocationSuggestion suggest = _locationSuggestionRepository.Get(_ => _.Id == suggestionId, _ => _.Locations);

                suggest.Status = RequestStatus.Approved;

                _locationSuggestionRepository.Update(suggest);
                ICollection<Location> locations = new List<Location>();
                var plan = _planRepository.Get(_ => _.Id == suggest.PlanId, _ => _.PlanLocations);

                foreach (var item in suggest.Locations)
                {
                    _planLocationRepository.Create(new PlanLocation
                    {
                        PlanId = suggest.PlanId,
                        LocationId = item.Id,
                        Done = false,
                        PlanDay = suggest.PlanDay ?? 0
                    });
                }

                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AcceptStatusLocationSuggestion), ex);

                return false;
            }
        }

        public bool AddReview(Review review)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    _reviewRepository.Create(review);

                    _unitOfWork.SaveChanges();
                    scope.Complete();
                    return true;
                }//end scope
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Create), ex);
                return false;
            }
        }

        protected MemoryStream ConvertToStream(string base64)
        {
            var imageParts = base64.Split(',').ToList<string>();
            byte[] bytes = Convert.FromBase64String(imageParts[1]);
            MemoryStream memoryStream = new MemoryStream(bytes);
            return memoryStream;
        }

        public async Task<bool> Create(Location location, Photo primaryPhoto, IEnumerable<Photo> photos, IEnumerable<BusinessHour> businessHours, int[] tagList)
        {
            try
            {
                base.Create(location);

                List<Tag> tags = new List<Tag>();
                tagList.ToList().ForEach(_ =>
                {
                    var ele = _tagRepository.Get(__ => __.Id == _);
                    tags.Add(ele);
                });
                location.Tags = tags;
                base.Update(location);

                var stream = ConvertToStream(primaryPhoto.Path);
                var task = new FirebaseStorage("its-g8.appspot.com")
                 .Child("photo")
                 .Child($"{Guid.NewGuid()}.jpg")
                 .PutAsync(stream);

                primaryPhoto.Path = await task;
                _photoRepository.Create(primaryPhoto);
                _unitOfWork.SaveChanges();
                _locationPhotoRepository.Create(new LocationPhoto
                {
                    LocationId = location.Id,
                    PhotoId = primaryPhoto.Id,
                    IsPrimary = true
                });

                foreach (var _ in photos.ToList())
                {
                    stream = ConvertToStream(_.Path);
                    task = new FirebaseStorage("its-g8.appspot.com")
                     .Child("photo")
                     .Child($"{Guid.NewGuid()}.jpg")
                     .PutAsync(stream);

                    _.Path = await task;
                    _photoRepository.Create(_);
                    _unitOfWork.SaveChanges();
                    _locationPhotoRepository.Create(new LocationPhoto
                    {
                        LocationId = location.Id,
                        PhotoId = _.Id,
                        IsPrimary = false
                    });
                }

                businessHours.ToList().ForEach(_ =>
                {
                    _businessHourRepository.Create(new BusinessHour
                    {
                        LocationId = location.Id,
                        Day = _.Day,
                        OpenTime = _.OpenTime,
                        CloseTime = _.CloseTime
                    });
                });

                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Create), ex);
                return false;
            }
        }

        public bool Delete(int locationId)
        {
            Location entity = Find(locationId);
            entity.IsDelete = true;
            return Update(entity);
        }

        public bool Edit(Location data, Photo priamryPhoto, IEnumerable<Photo> photos, IEnumerable<BusinessHour> businessHours, int[] tagList)
        {
            try
            {
                var location = _repository.Get(_ => _.Id == data.Id, _ => _.BusinessHours, _ => _.Photos);
                var photoLocationNe = location.Photos;
                foreach (var item in photoLocationNe.ToList())
                {
                    _locationPhotoRepository.Delete(item);
                }
                _unitOfWork.SaveChanges();

                location.IsClosed = data.IsClosed;
                location.IsVerified = data.IsVerified;
                location.Address = data.Address;
                location.AreaId = data.AreaId;
                location.Category = data.Category;
                location.EmailAddress = data.EmailAddress;
                location.Description = data.Description;
                location.Latitude = data.Latitude;
                location.Name = data.Name;


                base.Update(location);
                _unitOfWork.SaveChanges();

                List<Tag> tags = new List<Tag>();
                var businessHourNe = location.BusinessHours;

                foreach (var item in businessHourNe.ToList())
                {
                    _businessHourRepository.Delete(item);
                }
                _unitOfWork.SaveChanges();

                tagList.ToList().ForEach(_ =>
                {
                    var ele = _tagRepository.Get(__ => __.Id == _, __ => __.Locations);
                    tags.Add(ele);
                });
                location.Tags = tags;
                base.Update(location);
                _unitOfWork.SaveChanges();

                _photoRepository.Create(priamryPhoto);
                _unitOfWork.SaveChanges();
                _locationPhotoRepository.Create(new LocationPhoto
                {
                    LocationId = location.Id,
                    PhotoId = priamryPhoto.Id,
                    IsPrimary = true
                });

                photos.ToList().ForEach(_ =>
                {
                    _photoRepository.Create(_);
                    _unitOfWork.SaveChanges();
                    _locationPhotoRepository.Create(new LocationPhoto
                    {
                        LocationId = location.Id,
                        PhotoId = _.Id,
                        IsPrimary = false
                    });
                });

                businessHours.ToList().ForEach(_ =>
                {
                    _businessHourRepository.Create(new BusinessHour
                    {
                        LocationId = location.Id,
                        Day = _.Day,
                        OpenTime = _.OpenTime,
                        CloseTime = _.CloseTime
                    });
                });

                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Edit), ex);
                return false;
            }
        }

        public Location Find(int locationId, params Expression<Func<Location, object>>[] includes)
        {
            Location location = null;
            try
            {
                location = _repository.GetAsQueryable(_ => _.Id == locationId, includes);
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Find), ex);
            }

            return location;
        }

        public Location Find(string name, params Expression<Func<Location, object>>[] includes)
        {
            Location location = null;
            try
            {
                location = _repository.GetAsQueryable(_ => _.Name == name, includes);
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Find), ex);
            }

            return location;
        }

        public IQueryable<Location> GetAll(params Expression<Func<Location, object>>[] includes)
        {
            IQueryable<Location> locations = null;
            try
            {
                locations = _repository.GetAllAsQueryable(includes);
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(GetAll), ex);
            }

            return locations ?? Enumerable.Empty<Location>().AsQueryable();
        }

        public IEnumerable<string> GetCategories()
        {
            return GetAll().Select(_ => _.Category);
        }

        public IEnumerable<LocationSuggestion> GetLocationSuggestion(int userId)
        {
            List<LocationSuggestion> locationSuggestions = new List<LocationSuggestion>();
            Creator user = _creatorRepository.Get(_ => _.Id == userId, _ => _.CreatedGroups.Select(
                __ => __.Plans.Select(___ => ___.LocationSuggestion.Select(____ => ____.Locations))),
                _ => _.CreatedGroups.Select(__ => __.Plans.Select(___ => ___.LocationSuggestion.Select(____ => ____.User))));

            foreach (var ele in user.CreatedGroups)
            {
                foreach (var item in ele.Plans)
                {
                    locationSuggestions.AddRange(item.LocationSuggestion);

                }
            }

            return locationSuggestions.GroupBy(_ => _.Id).Select(_ => _.First());
        }

        public bool RejectStatusLocationSuggestion(int suggestionId)
        {
            try
            {
                LocationSuggestion suggest = _locationSuggestionRepository.Get(_ => _.Id == suggestionId);

                suggest.Status = RequestStatus.Rejected;
                _locationSuggestionRepository.Update(suggest);

                _unitOfWork.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(RejectStatusLocationSuggestion), ex);

                return false;
            }
        }

        public IQueryable<Location> Search(Expression<Func<Location, bool>> searchValue, params Expression<Func<Location, object>>[] includes)
        {
            IQueryable<Location> locations = null;
            try
            {
                locations = _repository.SearchAsQueryable(searchValue, includes);
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Search), ex);
            }

            return locations ?? Enumerable.Empty<Location>().AsQueryable();
        }

        public bool CreateChangeRequest(ChangeRequest cr)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    _changeRequestRepository.Create(cr);
                    _unitOfWork.SaveChanges();

                    scope.Complete();
                    return true;
                }//end scope
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(CreateChangeRequest), ex);
                return false;
            }
        }

        public bool CreateReport(Report report)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    _reportRepository.Create(report);
                    _unitOfWork.SaveChanges();

                    scope.Complete();
                    return true;
                }//end scope
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(CreateChangeRequest), ex);
                return false;
            }
        }

        public ChangeRequest FindChangeRequest(int id)
        {
            return _changeRequestRepository.Get(_ => _.Id == id);
        }

        public bool UpdateChageRequest(ChangeRequest cr)
        {
            _unitOfWork.SaveChanges();
            return true;
        }
    }
}