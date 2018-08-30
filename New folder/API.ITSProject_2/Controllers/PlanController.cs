namespace API.ITSProject_2.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Core.ObjectModels.Entities;
    using Core.ObjectModels.Entities.EnumType;
    using Core.ApplicationService.Business.IdentityService;
    using Core.ApplicationService.Business.LogService;
    using Core.ApplicationService.Business.PagingService;
    using Core.ApplicationService.Business.EntityService;
    using API.ITSProject_2.ViewModels;
    using Core.ApplicationService.Business.Algorithm;
    using Core.ObjectModels.Algorithm;
    using System.Data.Entity;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.ComponentModel.DataAnnotations;

    public class PlanController : _BaseController
    {
        private HttpClient client;
        private readonly IPlanService _planService;
        private readonly ILocationService _locationService;
        private readonly ITagService _tagService;
        private readonly ISearchTreeService _searchTreeService;

        public PlanController(ILoggingService loggingService, IPagingService paggingService,
            IIdentityService identityService, IPlanService planService, ILocationService locationService,
            IPhotoService photoService,
            ITagService tagService, ISearchTreeService searchTreeService) : base(loggingService, paggingService,
            identityService, photoService)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://exp.host");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            this._planService = planService;
            this._locationService = locationService;
            this._tagService = tagService;
            this._searchTreeService = searchTreeService;
        }

        #region Get

        [HttpGet]
        [Route("api/Plan/Details")]
        public async Task<IHttpActionResult> Details([FromUri] int id)
        {
            try
            {
                
                Plan plan = _planService.Find(id, _ =>
                        _.PlanLocations.Select(__ => __.Location)
                            .Select(___ => ___.Photos.Select(_____ => _____.Photo)),
                    _ => _.PlanLocations.Select(__ => __.Location.Reviews),
                    _ => _.Notes, _ => _.Area, _ => _.Voters,_ => _.Group);

                if (plan == null)
                    return BadRequest("Not found");
                else
                {
                    var temp = ModelBuilder.ConvertToPlanDetailViewModels(plan);
                    if (User.Identity.IsAuthenticated)
                    {
                        int userId = (await CurrentUser()).Id;
                        temp.IsOwner = temp.MemberId == userId;
                        temp.IsVoted = plan.Voters.Contains(await CurrentUser());
                        temp.IsGroupOwner = temp.GroupCreatorId == userId;
                    }
                    return Ok(temp);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Details), ex);

                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/FeaturedTrip")]
        public IHttpActionResult FeaturedTrip()
        {
            try
            {
                IQueryable<Plan> planList = _planService.GetFeaturedTrip();

                return Ok(ModelBuilder.ConvertToFeaturedPlanViewModels(planList));
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(FeaturedTrip), ex);

                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/MyPlans")]
        public async Task<IHttpActionResult> GetPlans()
        {
            try
            {
                int userId = (await CurrentUser()).Id;
                var plans = _planService.GetPlans(userId).ToList().Where(_ => !_.IsPublic && _.GroupId == null);

                return Ok(ModelBuilder.ConvertToMyPlan(plans, userId));
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(GetPlans), ex);

                return InternalServerError(ex);
            }
        }

        #endregion

        #region Post

        [HttpPost]
        [Authorize]
        [Route("api/Test2")]
        public async Task<IHttpActionResult> Algorithm(CreateSuggestedPlanViewModels viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            List<Tag> tags = _tagService.GetAll()
                .Where(tag => tag.Answer
                    .Any(answer => viewModel.Answers.Contains(answer.Id))
                )
                .ToList();

//            var path = CurrentContext.Server.MapPath("/Tree/tree.json");
            Tree tree = _searchTreeService.BuildTree(_locationService.GetAll().Include(location => location.Tags)
                .ToList());
            
//            if (tree == null)
//            {
//                tree = _searchTreeService.BuildTree(_locationService.GetAll().Include(location => location.Tags)
//                    .ToList());
//                _searchTreeService.WriteTree(path, tree);
//            }

            var resultIds = _searchTreeService.SearchTree(tags, tree);
            var locationsResult = _locationService.GetAll()
                .Include(location => location.Reviews)
                .Include(location => location.Tags)
                .Include(location => location.Photos.Select(__ => __.Photo))
                .Where(location => location.AreaId == viewModel.AreaId)
                .Where(location => resultIds.Contains(location.Id));

            List<TreeViewModels> resultList = new List<TreeViewModels>();
            foreach (Location location in locationsResult.Where(_ => !_.IsDelete))
            {
                var locationTags = location.Tags;
                var commonTags = locationTags.Intersect(tags);
                int ratingCount = location.Reviews.Count;
                var rating = location.Reviews.Sum(_ => _.Rating) / ratingCount;
                rating = float.IsNaN(rating) ? 0 : rating;

                foreach (Review review in location.Reviews)
                {
                    rating += review.Rating;
                }

                TreeViewModels result = new TreeViewModels
                {
                    Id = location.Id,
                    Address = location.Address,
                    Location = location.Name,
                    Percent = (tags.Count / commonTags.Count()).ToString(),
                    PrimaryPhoto = location.Photos.FirstOrDefault(_ => _.IsPrimary)?.Photo.Path.ToString(),
                    Rating = rating,
                    Reasons = commonTags.Select(tag => tag.Name).ToList(),
                    ReviewCount = location.Reviews.Count,
                    Categories = location.Category,
                    TotalTimeStay = location.TotalTimeStay
                };

                resultList.Add(result);
            }

            var locationListResult = resultList.OrderByDescending(_ => _.Reasons.Count).ThenByDescending(_ => _.Rating)
                .Select(model => new Core.ObjectModels.Entities.Helper.TreeViewModels
                {
                    Address = model.Address,
                    Categories = model.Categories,
                    Id = model.Id,
                    Location = model.Location,
                    Percent = model.Percent,
                    PrimaryPhoto = model.PrimaryPhoto,
                    Rating = model.Rating,
                    Reasons = model.Reasons
                })
                .ToList();


            int userId = (await CurrentUser()).Id;
            Plan resultPlan = await _planService.CreateSuggestedPlan(
                new Plan
                {
                    Name = viewModel.Name,
                    AreaId = viewModel.AreaId,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,    
                    CreatorId = userId,
                    MemberId = userId
                },
                locationListResult);

            return Ok(resultPlan.Id);
        }


        [HttpPost]
        [Route("api/Plan/AddSuggestion")]
        public async Task<IHttpActionResult> AddSuggestionToPlan(LocationSuggestionViewModels locationSuggestion)
        {
            try
            {
                int userId = (await CurrentUser()).Id;
                ICollection<Location> locations = new List<Location>();

                foreach (var item in locationSuggestion.LocationIds ?? new List<int>())
                {
                    var location = _locationService.Find(item);
                    if (location != null)
                        locations.Add(location);
                    else
                        return BadRequest("Location not existed");
                }

                var locationSuggest = new LocationSuggestion
                {
                    Comment = locationSuggestion.Comment,
                    Locations = locations, 
                    UserId = userId,
                    PlanId = locationSuggestion.PlanId,
                    Status = RequestStatus.NotYet,
                    PlanDay = locationSuggestion.PlanDay
                };
                bool result = _planService.Create(locationSuggest);

                var plan = _planService.Find(locationSuggestion.PlanId, _ => _.Group, _ => _.Creator, _ => _.Group.Creator);
                var creatorGroup = plan.Group.Creator;
                var content = new
                {
                    to = $"{creatorGroup.MobileToken}",
                    title = $"{(await CurrentUser()).FullName} muốn thêm { string.Join(", ", locations.Select(_ => _.Name)) } địa điểm vào chuyến đi {plan.Name}",
                    body = $"{locationSuggestion.Comment}",
                    data = new
                    {
                        type = "LocationSuggestion"
                    }
                };
                var temp = JsonConvert.SerializeObject(content);
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("/--/api/v2/push/send", content);

                if (result)
                    return Ok();
                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddNoteToPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/Plan/AddNote")]
        public IHttpActionResult AddNoteToPlan(CreatePlanNoteViewModles planNote)
        {
            try
            {
                Note note = ModelBuilder.ConvertToModels(planNote);

                if (!planNote.Index.HasValue)
                {
                    Plan plan = _planService.Find(planNote.PlanId, _ => _.Notes, _ => _.PlanLocations);

                    IEnumerable<PlanLocation> planLocations = plan.PlanLocations;
                    IEnumerable<Note> planNotes = plan.Notes;

                    int maxLocationIndex = planLocations.OrderByDescending(_ => _.Index).FirstOrDefault() != null
                        ? planLocations.OrderByDescending(_ => _.Index).FirstOrDefault().Index
                        : 0;
                    int maxPlanIndex = planNotes.OrderByDescending(_ => _.Index).FirstOrDefault() != null
                        ? planNotes.OrderByDescending(_ => _.Index).FirstOrDefault().Index
                        : 0;

                    note.Index = (maxLocationIndex > maxPlanIndex ? maxLocationIndex : maxPlanIndex) + 1;
                }

                bool result = _planService.Create(note);
                if (result)
                    return Ok(note.Id);
                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddNoteToPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> Post(CreatePlanViewModels plan)
        {
            try
            {
                int userId = (await CurrentUser()).Id;

                Plan tmpPlan = ModelBuilder.ConvertToModels(plan);
                tmpPlan.CreatorId = userId;
                tmpPlan.MemberId = userId;

                bool result = _planService.Create(tmpPlan);
                if (result)
                {
                    return Ok(tmpPlan.Id);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Post), ex);

                return InternalServerError(ex);
            }
        }

        #endregion

        #region Put

        public class EditNoteViewModels
        {
            [Required]
            public int NoteId { get; set; }

            [Required, MaxLength(255)]
            public string Title { get; set; }

            public string Description { get; set; }
        }
        [HttpPut]
        [Route("api/Plan/EditNote")]
        public IHttpActionResult EditNote(EditNoteViewModels note)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var editNote = _planService.FindNote(note.NoteId);
                editNote.Title = note.Title;
                editNote.Content = note.Description;

                _planService.UpdatePlanNote(editNote);

                return Ok();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(EditNote), ex);
                return InternalServerError();
            }
            
        }

        [HttpPut]
        [Authorize, Route("api/Plan/VotePlan")]
        public async Task<IHttpActionResult> VotePlan([FromBody]int planId)
        {
            try
            {
                User user = await CurrentUser();
                Plan plan = _planService.Find(planId, _ => _.Voters);
                if (plan.Voters.Contains(user))
                {
                    plan.Voters.Remove(user);
                } else
                {
                    plan.Voters.Add(user);
                }
                _planService.Update(plan);
                return Ok();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(VotePlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Plan/UpdatePlan")]
        public IHttpActionResult UpdatePlan(UpdatePlanViewModel data)
        {
            try
            {
                if (data == null || data.plan == null || data.updateIndexPlanLocationAndNote == null)
                    return BadRequest();
                Plan plan = _planService.Find(data.plan.Id);
                if (plan == null)
                    return BadRequest();
                plan.StartDate = data.plan.StartDate;
                plan.EndDate = data.plan.EndDate;
                plan.Name = data.plan.Name;

                _planService.Update(plan);

                try
                {
                    bool result = false;

                    foreach (var item in data.updateIndexPlanLocationAndNote.PlanLocation)
                    {
                        var planLocation = _planService.FindPlanLocation(item.Id);
                        planLocation.Index = item.Index;
                        planLocation.PlanDay = item.PlanDay;

                        result = _planService.UpdatePlanLocation(planLocation);
                    }

                    if (result)
                    {
                        foreach (var item in data.updateIndexPlanLocationAndNote.PlanNotes)
                        {
                            var planNote = _planService.FindNote(item.Id);
                            planNote.Index = item.Index;
                            planNote.PlanDay = item.PlanDay;

                            result = _planService.UpdatePlanNote(planNote);
                        }
                    }

                    if (result)
                        return Ok();
                    else
                        return BadRequest();
                }
                catch (Exception ex)
                {
                    _loggingService.Write(GetType().Name, nameof(EditIndexPlanLocationAndNote), ex);

                    return InternalServerError(ex);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(UpdatePlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Plan/PublicPlan")]
        public IHttpActionResult PublicPlan(int planId)
        {
            try
            {
                var plan = _planService.Find(planId, _ => _.PlanLocations);
                if (plan.PlanLocations.Count == 0)
                {
                    return BadRequest("Chuyến đi của bạn đang trống");
                }
                var temp = _planService.PublicPlan(planId);

                return Ok(temp.Id);
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(PublicPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Plan/EditIndexPlanLocationAndNote")]
        public IHttpActionResult EditIndexPlanLocationAndNote(
            UpdateIndexPlanLocationAndNote updateIndexPlanLocationAndNote)
        {
            try
            {
                bool result = false;

                foreach (var item in updateIndexPlanLocationAndNote.PlanLocation)
                {
                    var planLocation = _planService.FindPlanLocation(item.Id);
                    planLocation.Index = item.Index;
                    planLocation.PlanDay = item.PlanDay;

                    result = _planService.UpdatePlanLocation(planLocation);
                }

                if (result)
                {
                    foreach (var item in updateIndexPlanLocationAndNote.PlanNotes)
                    {
                        var planNote = _planService.FindNote(item.Id);
                        planNote.Index = item.Index;
                        planNote.PlanDay = item.PlanDay;

                        result = _planService.UpdatePlanNote(planNote);
                    }
                }

                if (result)
                    return Ok();
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(EditIndexPlanLocationAndNote), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Plan/SwitchStatusPlanNote")]
        public IHttpActionResult SwitchStatusPlanNote(int id)
        {
            try
            {
                Note planNote = _planService.FindNote(id);

                planNote.Done = !planNote.Done;
                bool result = _planService.UpdatePlanNote(planNote);

                if (result)
                    return Ok();
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddLocationToPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Plan/SwitchStatusPlanLocation")]
        public IHttpActionResult SwitchStatusPlanLocation(int id)
        {
            try
            {
                PlanLocation planLocation = _planService.FindPlanLocation(id);

                planLocation.Done = !planLocation.Done;
                bool result = _planService.UpdatePlanLocation(planLocation);

                if (result)
                    return Ok();
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddLocationToPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Plan/AddLocations")]
        public IHttpActionResult AddLocationToPlan(LocationInPlanViewModels planLocation)
        {
            try
            {
                PlanLocation location = ModelBuilder.ConvertToModels(planLocation);


                if (!planLocation.Index.HasValue)
                {
                    Plan plan = _planService.Find(planLocation.PlanId, _ => _.PlanLocations, _ => _.Notes);

                    IEnumerable<PlanLocation> planLocations = plan.PlanLocations;
                    IEnumerable<Note> planNotes = plan.Notes;

                    int maxLocationIndex = planLocations.OrderByDescending(_ => _.Index).FirstOrDefault() != null
                        ? planLocations.OrderByDescending(_ => _.Index).FirstOrDefault().Index
                        : 0;
                    int maxPlanIndex = planNotes.OrderByDescending(_ => _.Index).FirstOrDefault() != null
                        ? planNotes.OrderByDescending(_ => _.Index).FirstOrDefault().Index
                        : 0;

                    location.Index = (maxLocationIndex > maxPlanIndex ? maxLocationIndex : maxPlanIndex) + 1;
                } //end if identity max index

                bool result = _planService.AddLocationToPlan(location);

                if (result)
                    return Ok();
                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddLocationToPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Plan/UpdateIndexPlanLocation")]
        public IHttpActionResult UpdatePlanLocation(UpdatePlanLocationViewModels planLocation)
        {
            try
            {
                Note updateNote = _planService.FindNote(planLocation.PlanLocationId);
                updateNote.Index = planLocation.Index;
                bool result = _planService.UpdatePlanNote(updateNote);

                if (result)
                    return Ok();
                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddLocationToPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Plan/UpdateIndexPlanNote")]
        public IHttpActionResult UpdatePlanNote(UpdatePlanNoteViewModels noteLocation)
        {
            try
            {
                PlanLocation updatePlanLocation = _planService.FindPlanLocation(noteLocation.NoteId);
                updatePlanLocation.Index = noteLocation.Index;
                bool result = _planService.UpdatePlanLocation(updatePlanLocation);

                if (result)
                    return Ok();
                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddLocationToPlan), ex);

                return InternalServerError(ex);
            }
        }

        #endregion

        #region Delete

        [HttpDelete]
        [Route("api/Plan/DeleteNote")]
        public IHttpActionResult RemoveNote(int noteId)
        {
            try
            {
                bool result = _planService.DeleteNote(noteId);

                if (result)
                    return Ok();
                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddLocationToPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("api/Plan/AddLocations")]
        public IHttpActionResult RemoveLocationToPlan(int planId, int locationId)
        {
            try
            {
                bool result = _planService.DeletePlanLocation(planId, locationId);

                if (result)
                    return Ok();
                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(AddLocationToPlan), ex);

                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IHttpActionResult> Remove(int planId)
        {
            try
            {
                int userId = (await CurrentUser()).Id;

                Plan plan = _planService.Find(planId);
                if (plan != null)
                {
                    if (userId == plan.MemberId)
                    {
                        bool result = _planService.Delete(plan);

                        if (result)
                            return Ok();
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _loggingService.Write(GetType().Name, nameof(Remove), ex);

                return InternalServerError(ex);
            }
        }

        #endregion
    }
}