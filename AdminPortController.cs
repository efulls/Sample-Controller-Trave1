using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TraveUI.Libs;
using TraveUI.Models.CRUD;
using TraveUI.Models.Paging;
using TraveUI.Services.CRUD;
using TraveUI.Services.CRUD.Contract;
using TraveUI.ViewModel;
using Travewell.Clients;
using Travewell.Clients.Utils;
using Travewell.Model.Domain;
using Travewell.Model.DTO;
using Travewell.Model.DTO.FrontEnd;
using GenericPagingResponse = Travewell.Model.DTO.GenericPagingResponse;

namespace TraveUI.Controllers
{
    public class AdminPortController : _BaseCrudController
    {
        private SimpleConfiguration administrationSimpleConfiguration;
        private SimpleConfiguration commonSimpleConfiguration;

        private readonly IBranchesService _branchService;

        public AdminPortController()
        {
            _branchService = new BranchesService(this);
        }

        #region Branch
        /*
        [Crud("Branch", CrudType.Index)]
        public ActionResult BranchIndex()
        {
            return PageIndex(_branchService);
        }

        [Crud("Branch", CrudType.Create)]
        public ActionResult BranchCreate()
        {
            return PageCreate<BranchesUI>(PrepareBranchEditPage);
        }

        [Crud("Branch", CrudType.Update)]
        public ActionResult BranchUpdate(string id)
        {
            return PageUpdate(PrepareBranchEditPage, _branchService, id);
        }

        [Crud("Branch", CrudType.Delete)]
        public ActionResult BranchDelete(string id)
        {
            return PageDelete(_branchService, id);
        }

        [HttpPost]
        [Crud("Branch", CrudType.Create)]
        public ActionResult BranchCreate(BranchesUI request)
        {
            return PostCreate(PrepareBranchEditPage, _branchService, request);
        }

        [HttpPost]
        [Crud("Branch", CrudType.Update)]
        public ActionResult BranchUpdate(BranchesUI request)
        {
            return PostUpdate(PrepareBranchEditPage, _branchService, request);
        }

        public void PrepareBranchEditPage(BaseVM model)
        {

        }
        */
        #endregion

        private SimpleConfiguration GetCommonConfiguration()
        {
            if (commonSimpleConfiguration == null)
                commonSimpleConfiguration = new SimpleConfiguration
                {
                    apiUrl = GetConfigFromCache("ApiInHouseURL")
                };
            return commonSimpleConfiguration;
        }

        private SimpleConfiguration GetAdministrationConfiguration()
        {
            if (administrationSimpleConfiguration == null)
                administrationSimpleConfiguration = new SimpleConfiguration
                {
                    apiUrl = GetConfigFromCache("ApiInHouseURL")
                };
            return administrationSimpleConfiguration;
        }

        public ActionResult BranchPackageUpdate(string id)
        {
            var vm = new EditorPageVM<BranchDetailDto>("AdminPort", "BranchPackage", "Update Branch Package");

            try
            {
                var cred = Comn.GetLoginCredential();
                var service = new Administration(GetCommonConfiguration());

                var raw = service.GetBranchDetail(new GetBranchDetailRequest
                {
                    sessionId = cred.Signature,
                    branchCode = id
                });

                var dto = JsonHelper.Deserialize<BranchDetailDto>(raw.data);

                vm.EditorModel = dto;
                ViewBag.VM = vm;
                return View(vm.EditorModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult BranchIndex()
        {
            try
            {
                var cred = Comn.GetLoginCredential();
                var service = new Administration(GetAdministrationConfiguration());

                var param = LoadPaginationParameter(Request);
                var raw = service.GetBranchesList(new GetBranchesListRequest
                {
                    pagenumber = 1,
                    recordcount = 999,
                    sessionId = cred.Signature
                });

                var data = ToPaginationBranchesUI(raw);
                var vm = new IndexPageVM<BranchesUI>("AdminPort", "Branchs", "Branchs List", data);


                return View(vm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult BranchsCreate(string code)
        {
            var vm = new EditorPageVM<BranchDetailDto>("AdminPort", "Branchs", "Create Branchs");

            try
            {
                var cred = Comn.GetLoginCredential();
                var service = new Administration(GetAdministrationConfiguration());
                var dto = new BranchDetailDto()
                {
                    Code = code
                };

                vm.EditorModel = dto;
                ViewBag.VM = vm;
                return View(vm.EditorModel);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BranchsCreate(BranchDetailDto request)
        {

            var cred = Comn.GetLoginCredential();
            var service = new Administration(GetAdministrationConfiguration());
            var data = JsonHelper.Serialize<BranchDetailDto>(request);

            var response = service.CreateBranches(new CrudRequest
            {
                sessionId = cred.Signature,
                model = data
            });

            return RedirectToAction("UsersIndex", "AdminPort");
        }

        public ActionResult UsersIndex(string id="")
        {
            try
            {
                var cred = Comn.GetLoginCredential();
                var service = new Common(new SimpleConfiguration
                {
                    apiUrl = GetConfigFromCache("ApiInHouseURL")
                });

                var param = LoadPaginationParameter(Request);
                var raw = service.GetUsersList(new GetUserListRequest
                {
                    pagenumber = 1,
                    recordcount = 999,
                    sessionId = cred.Signature,
                    branchCode = id
                });

                var data = ToPaginationUserDto(raw);
                var vm = new IndexPageVM<DomUsers>("AdminPort", "Users", "User List",data);

                //vm.SetSelectListItem(ConstantVM.DataKeySearchIndexTable, GetSearchSelectListData(), param.SearchType);

                return View(vm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult UsersCreate()
        {
            var vm = new EditorPageVM<DomUsers>("AdminPort", "Users", "Create User");

            try
            {
                var cred = Comn.GetLoginCredential();
                var service = new Common(GetCommonConfiguration());

                // Get Data BranchUserGroup
                // Deserialize model.data to List
                // Masukkan ke VM. untuk akses 

                var items = service.GetAirlineList(new GetAirlineListRequest
                { 
                    pagenumber = 1,
                    recordcount = 99,
                    sessionId = cred.Signature
                });
                var branchs = JsonHelper.Deserialize<List<DomAirline>>(items.data);

                var selectBranch = new Dictionary<string, string>();
                foreach(var branch in branchs)
                {
                    selectBranch.Add(branch.CarrierCode, branch.Name);
                }
                vm.SetSelectListItem("branchusergroupbyid", selectBranch, "");
                //=========================
                //Make this login data

                var myprofile = service.GetUsersById(new GetByIdRequest
                {
                    sessionId = cred.Signature,
                    id = (cred.UserId).ToString()
                });

                var profiles = JsonHelper.Deserialize<DomUsers>(myprofile.data);

                //TO DO: Make this data from user login
                // var ? branchusergroupbyid
                // var ? CurrentBranchLocationId
                //==========================

                var dto = new DomUsers()
                {
                    Id = 0
                };


                //vm.Bind(new DomUsers());
                //vm["CurrentBranchLocationId"].Value = "Hallo Broo";


                vm.EditorModel = dto;

                vm.EditorModel.CurrentBranchLocationId = profiles.CurrentBranchLocationId;

                ViewBag.VM = vm;
                return View(vm.EditorModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UsersCreate(DomUsers request)
        {

            var cred = Comn.GetLoginCredential();
            var service = new Common(GetCommonConfiguration());
            var data = JsonHelper.Serialize<DomUsers>(request);

            var response = service.CreateUsers(new CrudRequest
            {
                sessionId = cred.Signature,
                model = data
            });

            return RedirectToAction("UsersIndex", "AdminPort");
        }

        public ActionResult UsersUpdate(string id)
        {
            var vm = new EditorPageVM<DomUsers>("AdminPort", "Users", "Update User");

            try
            {
                var cred = Comn.GetLoginCredential();
                var service = new Common(GetCommonConfiguration());

                var raw = service.GetUsersById(new GetByIdRequest
                {
                    sessionId = cred.Signature,
                    id = id
                });

                var dto = JsonHelper.Deserialize<DomUsers>(raw.data);

                vm.SelectListItemDictionary.Add("BranchUserGroupIdSelectList", BranchUserGroupList(dto.BranchCode, dto.BranchUserGroupId));

                vm.EditorModel = dto;
                ViewBag.VM = vm;
                return View(vm.EditorModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UsersUpdate(DomUsers request)
        {
            var cred = Comn.GetLoginCredential();
            var service = new Common(GetCommonConfiguration());
            var data = JsonHelper.Serialize<DomUsers>(request);
            
            var response = service.UpdateUsers(new CrudRequest
            {
                sessionId = cred.Signature,
                model = data
            });

            return RedirectToAction("UsersIndex", "AdminPort");
        }

        public ActionResult UsersDelete(DomUsers request)
        {
            var cred = Comn.GetLoginCredential();
            var service = new Common(GetCommonConfiguration());
            var data = JsonHelper.Serialize<DomUsers>(request);

            var response = service.DeleteUsers(new CrudRequest
            {
                sessionId = cred.Signature,
                model = data 
            });

            return RedirectToAction("UsersIndex", "AdminPort");
        }
        //TODO Update Branch Package

        //As Agent View Method
        public ActionResult MyBranchView()
        {
            var vm = new EditorPageVM<BranchDetailDto>("AdminPort", "Branch", "My Branch View");

            try
            {
                var cred = Comn.GetLoginCredential();
                var service = new Administration(GetCommonConfiguration());

                var raw = service.GetBranchDetail(new GetBranchDetailRequest
                {
                    sessionId = cred.Signature,
                    branchCode = cred.BranchCode
                });

                var dto = JsonHelper.Deserialize<BranchDetailDto>(raw.data);

                vm.EditorModel = dto;
                ViewBag.VM = vm;
                return View("MyBranchView", vm.EditorModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private CreateBranchRequest ToCreateBranchRequest(BranchDetailDto dto)
        {
            var retval = new CreateBranchRequest
            {
                Code = dto.Code,
                Name = dto.Name,
                Npwp = dto.Npwp,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                AddressLine3 = dto.AddressLine3,
                City = dto.City,
                ZipCode = dto.ZipCode,
                //BranchParent = dto.BranchParent,
                IsActive = dto.IsActive,
                BranchTypeCode = dto.BranchTypeCode,
                //PostBookingNotificationUrl = dto.PostBookingNotificationUrl,
                //PostIssueNotificationUrl = dto.PostIssueNotificationUrl,
                //Attribute1 = dto.Attribute1,
                //Attribute2 = dto.Attribute2,
                //Attribute3 = dto.Attribute3,
                //Attribute4 = dto.Attribute4,
                //CorporateCode = dto.CorporateCode,
                BranchContactInformation = new BranchContactInformationRequest
                {
                    Title = dto.Title,
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName,
                    LastName = dto.LastName,
                    PhoneMobile = dto.PhoneMobile,
                    Email = dto.Email
                }
            };

            return retval;
        }

        private Pagination<BranchIndexDto> ToPaginationBranchIndexDto(GenericPagingResponse raw)
        {
            var page = new Pagination<BranchIndexDto>();
            page.PageActive = raw.pagenumber;
            page.TotalRows = raw.totalrecordcount;
            page.ValueItems = (string.IsNullOrEmpty(raw.data)) ? new List<BranchIndexDto>() : JsonHelper.DeserializeObject<IEnumerable<BranchIndexDto>>(raw.data);
            return page;
        }

        private Pagination<BranchesUI> ToPaginationBranchesUI(GenericPagingResponse raw)
        {
            var page = new Pagination<BranchesUI>();
            page.PageActive = raw.pagenumber;
            page.TotalRows = raw.totalrecordcount;
            page.ValueItems = (string.IsNullOrEmpty(raw.data)) ? new List<BranchesUI>() : JsonHelper.DeserializeObject<IEnumerable<BranchesUI>>(raw.data);
            return page;
        }

        private Pagination<DomUsers> ToPaginationUserDto(GenericPagingResponse raw)
        {
            var page = new Pagination<DomUsers>();
            page.PageActive = raw.pagenumber;
            page.TotalRows = raw.totalrecordcount;
            page.ValueItems = (string.IsNullOrEmpty(raw.data)) ? new List<DomUsers>() : JsonHelper.DeserializeObject<IEnumerable<DomUsers>>(raw.data);
            return page;
        }
    }
}