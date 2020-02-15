using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TraveUI.Models.Paging;
using TraveUI.Services.CRUD;
using TraveUI.Services.CRUD.Contract;
using TraveUI.ViewModel;
using Travewell.Clients;
using Travewell.Clients.Utils;
using Travewell.Model.Domain;
using Travewell.Model.DTO;

namespace TraveUI.Controllers
{
    public class AdminExampleController : _BaseCrudController
    {
        private SimpleConfiguration commonSimpleConfiguration;
        // GET: AdminExample
        public ActionResult UsersIndex(int page = 1, string branch = "")
        {
            try
            {
                var cred = Comn.GetLoginCredential();
                var service = new Common(GetCommonConfiguration());

                var param = LoadPaginationParameter(Request);
                var raw = service.GetUsersList(new GetUserListRequest
                {
                    pagenumber = page,
                    recordcount = 2,
                    sessionId = cred.Signature,
                    branchCode = branch
                });

                var data = ToPaginationUserDto(raw);
                var vm = new IndexPageVM<DomUsers>("AdminPort", "Users", "User List", data);

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
                var dto = new DomUsers()
                {
                    Id = 0
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
        public ActionResult UsersCreate(DomUsers request)
        {
            return View();
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

        private SimpleConfiguration GetCommonConfiguration()
        {
            if (commonSimpleConfiguration == null)
                commonSimpleConfiguration = new SimpleConfiguration
                {
                    apiUrl = GetConfigFromCache("ApiInHouseURL")
                };
            return commonSimpleConfiguration;
        }
        private Pagination<DomUsers> ToPaginationUserDto(GenericPagingResponse raw)
        {
            var page = new Pagination<DomUsers>();
            page.PageActive = raw.pagenumber;
            page.TotalRows = raw.totalrecordcount;
            page.ValueItems = (string.IsNullOrEmpty(raw.data))
                ? new List<DomUsers>()
                : JsonHelper.DeserializeObject<IEnumerable<DomUsers>>(raw.data);
            return page;
        }
    }
}