namespace API.ITSProject_2.ViewModels
{
    using System.Linq;
    using System.Collections.Generic;
    using Core.ObjectModels.Entities;
    using Infrastructure.Identity.Models;
    using System;

    public class _ModelBuilder
    {

        #region Group
        public GroupLocationSuggestionViewModels ConvertToGroupLocationSuggestionViewModels(LocationSuggestion locationSuggestion)
        {
            //need to edit
            return new GroupLocationSuggestionViewModels
            {
                UserId = locationSuggestion.UserId,
                UserAvatar = locationSuggestion.User.Avatar,
                FullName = locationSuggestion.User.FullName,
                Comment = locationSuggestion.Comment,
                //LocationId = locationSuggestion.LocationId,
                //LocationName = locationSuggestion.Locations.Name,
                //LocationPhoto = locationSuggestion.Locations.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Id.ToString(),
                PlanId = locationSuggestion.PlanId,
                PlanName = locationSuggestion.Plan.Name,
                Status = (int)locationSuggestion.Status
            };
        }

        public List<GroupLocationSuggestionViewModels> ConvertToGroupLocationSuggestionViewModels(IEnumerable<LocationSuggestion> locationSuggestion)
        {
            List<GroupLocationSuggestionViewModels> result = new List<GroupLocationSuggestionViewModels>();
            locationSuggestion.ToList().ForEach(_ => result.Add(ConvertToGroupLocationSuggestionViewModels(_)));

            return result;
        }

        public GroupDetailViewModels ConvertToGroupDetailViewModel(Group group, int userId)
        {
            return new GroupDetailViewModels
            {
                Name = group.Name,
                Creator = ConvertToGroupDetailUserViewModels(group.Creator),
                Plans = ConvertToMyPlan(group.Plans, userId),
                Users = ConvertToGroupDetailUserViewModels(group.Members),
                IsOwner = userId == group.CreatorId
            };
        }

        public IEnumerable<GroupDetailViewModels> ConvertToGroupDetailUserViewModels(IEnumerable<Group> groups, int userId)
        {
            List<GroupDetailViewModels> result = new List<GroupDetailViewModels>();
            groups.ToList().ForEach(_ => result.Add(ConvertToGroupDetailViewModel(_, userId)));

            return result;
        }

        public GroupDetailUserViewModels ConvertToGroupDetailUserViewModels(User user)
        {
            return new GroupDetailUserViewModels
            {
                Id = user.Id,
                Name = user.FullName,
                Avatar = user.Avatar,
                EmailAddress = user.Id.ToString(),
                PhoneNumber = user.Id.ToString()
            };
        }

        public IEnumerable<GroupDetailUserViewModels> ConvertToGroupDetailUserViewModels(IEnumerable<User> users)
        {
            List<GroupDetailUserViewModels> result = new List<GroupDetailUserViewModels>();
            users.ToList().ForEach(_ => result.Add(ConvertToGroupDetailUserViewModels(_)));

            return result;
        }

        public GroupViewModels ConvertToGroupViewModels(Group group, int userId)
            => new GroupViewModels
            {
                Id = group.Id,
                Name = group.Name,
                MemberCount = group.Members.Count(),
                PlanCount = group.Plans.Count(),
                IsOwner = group.CreatorId == userId
            };

        public IEnumerable<GroupViewModels> ConvertToGroupViewModels(IEnumerable<Group> groups, int userId)
        {
            List<GroupViewModels> result = new List<GroupViewModels>();
            groups.ToList().ForEach(_ => result.Add(ConvertToGroupViewModels(_, userId)));

            return result;
        }
        #endregion

        #region Plan
        public Note ConvertToModels(CreatePlanNoteViewModles planNote)
            => new Note
            {
                PlanId = planNote.PlanId,
                Content = planNote.Content,
                Done = false,
                Index = planNote.Index ?? 0,
                Title = planNote.Title,
                PlanDay = planNote.PlanDay ?? 0
            };

        public Location ConvertToModels(EditLocationViewModels model)
           => new Location
            {
                Address = model.Address,
                AreaId = model.AreaId.Value,
                Description = model.Description,
                EmailAddress = model.Email,
                IsClosed = model.IsClosed,
                IsVerified = model.IsVerified,
                Latitude = model.Latitude.Value,
                Longitude = model.Longitude.Value,
                Name = model.Name,
                PhoneNumber = model.Phone,
                Website = model.Website,
                Category = model.Category
            };

        public Plan ConvertToModels(CreatePlanViewModels planLocation)
            => new Plan
            {
                EndDate = planLocation.EndDate,
                StartDate = planLocation.StartDate,
                Name = planLocation.Name,
                AreaId = planLocation.AreaId,
                IsPublic = false
            };

        public PlanLocation ConvertToModels(LocationInPlanViewModels planLocation)
            => new PlanLocation
            {
                Done = false,
                Comment = planLocation.Comment,
                LocationId = planLocation.LocationId,
                PlanId = planLocation.PlanId,
                PlanDay = planLocation.PlanDay ?? 0,
                Index = planLocation.Index ?? 0
            };

        public PlanDetailViewModels ConvertToPlanDetailViewModels(Plan plan)
        {
            return new PlanDetailViewModels
            {
                Id = plan.Id,
                Name = plan.Name,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Locations = ConvertToPlanDetailLocationViewModels(plan.PlanLocations),
                Notes = ConvertToPlanDetailNoteViewModels(plan.Notes),
                AreaId = plan.AreaId,
                AreaName = plan.Area.Name,
                MemberId = plan.MemberId.HasValue ? plan.MemberId.Value : -1,
                IsPublic = plan.IsPublic,
                GroupName = plan.Group == null ? null : plan.Group.Name,
                GroupCreatorId = plan.Group == null ? -1 : plan.Group.CreatorId
            };
        }

        public PlanDetailNoteViewModels ConvertToPlanDetailNoteViewModels(Note note)
        {
            return new PlanDetailNoteViewModels
            {
                Id = note.Id,
                Description = note.Content,
                Name = note.Title,
                Index = note.Index,
                PlanDay = note.PlanDay,
                IsDone = note.Done
            };
        }

        public IEnumerable<PlanDetailNoteViewModels> ConvertToPlanDetailNoteViewModels(IEnumerable<Note> planNotes)
        {
            IList<PlanDetailNoteViewModels> result = new List<PlanDetailNoteViewModels>();
            planNotes.ToList().ForEach(_ => result.Add(ConvertToPlanDetailNoteViewModels(_)));

            return result;
        }

        public PlanDetailLocationViewModels ConvertToPlanDetailLocationViewModels(PlanLocation planLocation)
        {
            int ratingCount = planLocation.Location.Reviews.Count;
            float rating = planLocation.Location.Reviews.Sum(_ => _.Rating) / ratingCount;
            rating = float.IsNaN(rating) ? 0 : rating;

            return new PlanDetailLocationViewModels
            {
                PlanLocationId = planLocation.Id,
                LocationId = planLocation.LocationId,
                Address = planLocation.Location.Address,
                Photo = planLocation.Location.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Path.ToString(),
                Title = planLocation.Location.Name,
                Rating = rating,
                ReviewCount = ratingCount,
                Index = planLocation.Index,
                PlanDay = planLocation.PlanDay,
                Category = planLocation.Location.Category,
                IsDone = planLocation.Done,
                TotalTimeStay = planLocation.Location.TotalTimeStay
            };
        }

        public IEnumerable<PlanDetailLocationViewModels> ConvertToPlanDetailLocationViewModels(IEnumerable<PlanLocation> planLocations)
        {
            IList<PlanDetailLocationViewModels> result = new List<PlanDetailLocationViewModels>();
            planLocations.ToList().ForEach(_ => result.Add(ConvertToPlanDetailLocationViewModels(_)));

            return result;
        }

        public MyPlanViewModels ConvertToMyPlan(Plan plan, int currentUserId)
        {
            return new MyPlanViewModels
            {
                Id = plan.Id,
                EndDate = plan.EndDate,
                StartDate = plan.StartDate,
                Name = plan.Name,
                Locations = ConvertToPlanLocationViewModels(plan.PlanLocations.Select(_ => _.Location).GroupBy(_ => _.Id).Select(_ => _.First())),
                AreaId = plan.AreaId,
                AreaName = plan.Area.Name,
                GroupName = plan.Group == null ? "" : plan.Group.Name,
                IsGroupOwner = currentUserId == (plan.Group != null ? plan.Group.CreatorId : -1),
                IsPlanOwner = currentUserId == plan.MemberId,
                IsPublic = plan.IsPublic
            };
        }

        public IEnumerable<MyPlanViewModels> ConvertToMyPlan(IEnumerable<Plan> plans, int userId)
        {
            IList<MyPlanViewModels> result = new List<MyPlanViewModels>();
            plans.ToList().ForEach(_ => result.Add(ConvertToMyPlan(_, userId)));

            return result;
        }

        public FeaturedPlanViewModels ConvertToFeaturedPlanViewModels(Plan plan)
            => new FeaturedPlanViewModels
            {
                Id = plan.Id,
                Name = plan.Name,
                Time = (plan.EndDate - plan.StartDate).Days + 1,
                Voter = plan.Voters.Count,
                Photo = plan.PlanLocations.FirstOrDefault()?.Location.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Path.ToString(),
                AreaId = plan.AreaId,
                AreaName = plan.Area.Name,
                CreatorId = plan.CreatorId,
                CreatorName = plan.Creator.FullName
            };

        public IEnumerable<FeaturedPlanViewModels> ConvertToFeaturedPlanViewModels(IEnumerable<Plan> plans)
        {
            IList<FeaturedPlanViewModels> result = new List<FeaturedPlanViewModels>();
            plans.ToList().ForEach(_ => result.Add(ConvertToFeaturedPlanViewModels(_)));

            return result;
        }
        #endregion

        #region Area
        public AreaDetailViewModels ConvertToAreaDetailsViewModels(Area area)
        {
            AreaDetailViewModels tmpArea = new AreaDetailViewModels
            {
                CoverPhoto = area.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Path,
                Id = area.Id,
                Name = area.Name,
                Locations = new List<CategoriesLocationCounter>(),
                FeaturedLocation = ConvertToPlanLocationViewModels(area.Locations.Where(_ => !_.IsDelete)).OrderByDescending(_ => _.Rating).Take(10).ToList(),
                FeaturedPlan = ConvertToFeaturedPlanViewModels(area.Plans.Where(_ => _.IsPublic)).OrderByDescending(_ => _.Voter).Take(10).ToList()
            };
            foreach (var item in area.Locations)
            {
                if (!tmpArea.Locations.Select(_ => _.Name).Contains(string.IsNullOrEmpty(item.Category) ? "Other" : item.Category))
                {
                    tmpArea.Locations.Add(new CategoriesLocationCounter
                    {
                        Name = string.IsNullOrWhiteSpace(item.Category) ? "Other" : item.Category,
                        Counter = area.Locations.Where(__ => __.Category == item.Category).Count()
                    });
                }
            }


            return tmpArea;
        }

        public AreaViewModels ConvertToAreaViewModels(Area area)
            => new AreaViewModels
            {
                Id = area.Id,
                Name = area.Name,
                LocationCount = area.Locations.Where(_ => !_.IsDelete).Count(),
                QuestionCount = area.Questions.Count
            };

        public IEnumerable<AreaViewModels> ConvertToAreaViewModels(IEnumerable<Area> area)
        {
            List<AreaViewModels> result = new List<AreaViewModels>();
            area.ToList().ForEach(_ => result.Add(ConvertToAreaViewModels(_)));

            return result;
        }


        public AreaFeatureViewModels ConvertToAreaFeaturedViewModels(Area area)
        {
            int planCount = 0;

            foreach (var item in area.Locations)
            {
                var temp = item.PlanLocations.Select(_ => _.Plan).Where(__ => __.IsPublic);
                planCount += temp.Count();
            }
            area.Locations.Where(_ => _.IsDelete).ToList().ForEach(_ =>
            {
                planCount += _.PlanLocations.Select(___ => ___.Plan).Where(__ => __.IsPublic).Count();
            });// Area must be includes locations and plan

            return new AreaFeatureViewModels
            {
                Id = area.Id,
                Name = area.Name,
                LocationCount = area.Locations.Where(_ => !_.IsDelete).Count(),
                PlanCount = planCount,
                Photo = area.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Path
            };
        }

        public IEnumerable<AreaFeatureViewModels> ConvertToAreaFeaturedViewModels(IEnumerable<Area> area)
        {
            List<AreaFeatureViewModels> result = new List<AreaFeatureViewModels>();
            area.ToList().ForEach(_ => result.Add(ConvertToAreaFeaturedViewModels(_)));

            return result;
        }
        #endregion

        #region Location
        public LocationDetailViewModels ConvertToLocationDetailViewModels(Location location)
        {
            int ratingCount = location.Reviews.Count;   
            float rating = location.Reviews.Sum(_ => _.Rating) / ratingCount;
            rating = float.IsNaN(rating) ? 0 : rating;

            return new LocationDetailViewModels
            {
                Address = location.Address,
                EmailAddress = location.EmailAddress,
                Id = location.Id,
                Name = location.Name,
                Website = location.Website,
                PhoneNumber = location.PhoneNumber,
                Rating = rating,
                RatingCount = ratingCount,
                BusinessHours = ConvertToBusinessHourViewModels(location.BusinessHours),
                Tags = location.Tags.Select(_ => new TagViewModels
                {
                    Id = _.Id,
                    Name = _.Name,
                    Categories = _.Categories
                }),
                PrimaryPhoto = location.Photos.OrderByDescending(_ => _.PhotoId).FirstOrDefault(_ => _.IsPrimary)?.Photo.Path,
                OtherPhotos = location.Photos.Where(_ => !_.IsPrimary).Select(_ => _.Photo).Select(_ => _.Path),
                Comments = ConvertToCommentViewModels(location.Reviews).OrderByDescending(_ => _.Id).Take(5),
                Category = location.Category,
                Area = location.AreaId.ToString(),
                Description = location.Description,
                IsClose = location.IsClosed,
                IsVerified = location.IsVerified,
                Lat = location.Latitude,
                Long = location.Longitude
            };
        }

        public PlanLocationViewModels ConvertToPlanLocationViewModels(Location location)
        {
            int ratingCount = location.Reviews.Count;   
            float rating = location.Reviews.Sum(_ => _.Rating) / ratingCount;
            rating = float.IsNaN(rating) ? 0 : rating;
            return new PlanLocationViewModels
            {
                Address = location.Address,
                Id = location.Id,
                Name = location.Name,
                Photo = location.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Path,
                Rating = rating,
                Category = location.Category
            };
        }

        public IEnumerable<PlanLocationViewModels> ConvertToPlanLocationViewModels(IEnumerable<Location> data)
        {
            List<PlanLocationViewModels> result = new List<PlanLocationViewModels>();
            data.ToList().ForEach(_ => result.Add(ConvertToPlanLocationViewModels(_)));

            return result;
        }

        public LocationViewModels ConvertToViewModels(Location location)
        {
            int ratingCount = location.Reviews.Count;
            float rating = location.Reviews.Sum(_ => _.Rating) / ratingCount;
            rating = float.IsNaN(rating) ? 0 : rating;

            return new LocationViewModels
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                EmailAddress = location.EmailAddress,
                IsClosed = location.IsClosed,
                IsVerified = location.IsVerified,
                PhoneNumber = location.PhoneNumber,
                Website = location.Website,
                AreaName = location.Area.Name,
                Categories = location.Category,
                Rating = rating,
                ReviewCount = ratingCount,
                PrimaryPhoto = location.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Path,
                Lat = location.Latitude,
                Long = location.Longitude,
                Range = string.Empty
            };
        }

        public LocationViewModels ConvertToViewModels(Location location, string range)
        {
            int ratingCount = location.Reviews.Count;
            float rating = location.Reviews.Sum(_ => _.Rating) / ratingCount;
            rating = float.IsNaN(rating) ? 0 : rating;

            return new LocationViewModels
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                EmailAddress = location.EmailAddress,
                IsClosed = location.IsClosed,
                IsVerified = location.IsVerified,
                PhoneNumber = location.PhoneNumber,
                Website = location.Website,
                AreaName = location.Area.Name,
                Categories = location.Category,
                Rating = rating,
                ReviewCount = ratingCount,
                PrimaryPhoto = location.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Path,
                Lat = location.Latitude,
                Long = location.Longitude,
                Range = range
            };
        }


        public IEnumerable<LocationViewModels> ConvertToViewModels(IEnumerable<Location> data)
        {
            List<LocationViewModels> result = new List<LocationViewModels>();
            data.ToList().ForEach(_ => result.Add(ConvertToViewModels(_)));

            return result;
        }

        public Location ConvertToModels(CreateLocationViewModels model)
        {
            return new Location
            {
                Address = model.Address,
                AreaId = model.AreaId.Value,
                Description = model.Description,
                EmailAddress = model.EmailAddress,
                IsClosed = model.IsClosed,
                IsDelete = false,
                IsVerified = model.IsVerified,
                Latitude = model.Latitude.Value,
                Longitude = model.Longitude.Value,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Website = model.Website,
                Category = model.Category,
                LocationRadius = model.Radius,
                TotalStayCount = 0,
                TotalTimeStay = 0
            };
        }
        #endregion

        #region Photo
        public Photo ConvertToModels(string base64, int userId)
        {
            return new Photo
            {
                UserId = userId,
                Path = base64
            };
        }

        public IEnumerable<Photo> ConvertToModels(IEnumerable<string> data, int userId)
        {
            List<Photo> result = new List<Photo>();
            data.ToList().ForEach(_ => result.Add(ConvertToModels(_, userId)));

            return result;
        }
        #endregion

        #region Tag
        public TagViewModels ConvertToViewModels(Tag tag)
            => new TagViewModels
            {
                Id = tag.Id,
                LocationCount = tag.Locations.Count,
                Name = tag.Name,
                Categories = tag.Categories
            };

        public IEnumerable<TagViewModels> ConvertToViewModels(IEnumerable<Tag> tag)
        {
            List<TagViewModels> result = new List<TagViewModels>();
            tag.ToList().ForEach(_ => result.Add(ConvertToViewModels(_)));

            return result;
        }

        public Tag ConvertToModels(CreateTagViewModels tag)
            => new Tag
            {
                Name = tag.Name,
                Categories = tag.Categories
            };
        #endregion

        #region Question
        public QuestionViewModels ConvertToViewModels(Question question)
            => new QuestionViewModels
            {
                Id = question.Id,
                AnswerCount = question.Answers.Count,
                Categories = question.Categories,
                Content = question.Content
            };

        public IEnumerable<QuestionViewModels> ConvertToViewModels(IEnumerable<Question> question)
        {
            List<QuestionViewModels> result = new List<QuestionViewModels>();
            question.ToList().ForEach(_ => result.Add(ConvertToViewModels(_)));

            return result;
        }

        public Question ConvertToModels(CreateQuestionViewModels question)
            => new Question
            {
                Content = question.Content,
                Categories = question.Categories
            };

        public QuestionDetailsViewModels ConvertToQuestionDetailsViewModels(Question question)
            => new QuestionDetailsViewModels
            {
                Answer = question.Answers.Select(_ => new AnswerViewModels
                {
                    Content = _.Content,
                    Id = _.Id,
                    Tags = _.Tags.Select(__ => new TagViewModels
                    {
                        Id = __.Id,
                        Name = __.Name
                    })
                }),
                Content = question.Content,
                Id = question.Id,
                Category = question.Categories
            };

        public IEnumerable<QuestionDetailsViewModels> ConvertToQuestionDetailsViewModels(IEnumerable<Question> question)
        {
            List<QuestionDetailsViewModels> result = new List<QuestionDetailsViewModels>();
            question.ToList().ForEach(_ => result.Add(ConvertToQuestionDetailsViewModels(_)));

            return result;
        }
        #endregion

        #region Business hour
        public ICollection<BusinessHour> ConvertToModels(IEnumerable<BusinessHourViewModels> data)
        {
            ICollection<BusinessHour> businessHours = new List<BusinessHour>();
            data = data ?? new List<BusinessHourViewModels>();
            data.ToList().ForEach(_ => businessHours.Add(ConvertToModels(_)));

            return businessHours;
        }

        public BusinessHour ConvertToModels(BusinessHourViewModels day)
            => new BusinessHour
            {
                Day = day.Day,
                OpenTime = day.From,
                CloseTime = day.To
            };

        public BusinessHourViewModels ConvertToBusinessHourViewModels(BusinessHour businessHour)
            => new BusinessHourViewModels
            {
                Day = businessHour.Day,
                From = businessHour.OpenTime,
                To = businessHour.CloseTime
            };

        public ICollection<BusinessHourViewModels> ConvertToBusinessHourViewModels(IEnumerable<BusinessHour> data)
        {
            ICollection<BusinessHourViewModels> businessHours = new List<BusinessHourViewModels>();
            data.ToList().ForEach(_ => businessHours.Add(ConvertToBusinessHourViewModels(_)));

            return businessHours;
        }
        #endregion

        #region Comment
        public CommentViewModels ConvertToCommentViewModels(Review review)
            => new CommentViewModels
            {
                Rating = review.Rating,
                Avatar = review.Creator.Avatar,
                Description = review.Description,
                Id = review.Id,
                Title = review.Title,
                CreatorName = review.Creator.FullName,
                Photos = review.Photos.Select(_ => _.Path),
                LocationId = review.LocationId
            };

        public IEnumerable<CommentViewModels> ConvertToCommentViewModels(IEnumerable<Review> reviews)
        {
            ICollection<CommentViewModels> comments = new List<CommentViewModels>();
            reviews.ToList().ForEach(_ => comments.Add(ConvertToCommentViewModels(_)));

            return comments;
        }
        #endregion

        #region User
        public SearchUserViewModels ConvertToSearchUserViewModels(User u, Account a)
            => new SearchUserViewModels
            {
                Address = u.Address,
                Avatar = u.Avatar,
                Birthdate = u.Birthdate,
                IsBanned = a.LockoutEnabled,
                EmailAddress = a.Email,
                Name = u.FullName,
                PhoneNumber = a.PhoneNumber,
                Id = u.Id
            };
        #endregion
    }
}