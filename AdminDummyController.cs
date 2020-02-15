using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TraveUI.Models.CRUD;
using TraveUI.Models.Paging;
using TraveUI.ViewModel;
using Travewell.Clients;
using Travewell.Clients.Utils;
using Travewell.Model.Domain;
using Travewell.Model.DTO;

namespace TraveUI.Controllers
{
    public class AdminDummyController: _BaseController
    {
        public ActionResult UserIndex(string sb, string sv, int page = 1)
        {
            var config = new SimpleConfiguration()
            {
                apiUrl = GetConfigFromCache("ApiInHouseURL")
            };
            var serviceCommon = new Common(config);
            var loginResponse = GetLoginCredential();
            var request = new GetUserListRequest()
            {
                searchBy = sb,
                searchValue = sv,
                pagenumber = page,
                recordcount = 4,
                branchCode = "",
                sessionId = loginResponse.Signature
            };

            var response = serviceCommon.GetUsersList(request);

            ViewBag.Request = request;

            //var data = ToPaginationUserDto(response);
            //var vm = new IndexPageVM<DomUsers>("AdminPort", "Users", "User List", data);

            return View(response);
        }

        public ActionResult UserCreate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserCreate(DomUsers request)
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserUpdate(DomUsers data)
        {
            return View();   
        }

        public ActionResult UserUpdate(string id)
        {
            var config = new SimpleConfiguration()
            {
                apiUrl = GetConfigFromCache("ApiInHouseURL")
            };
            var serviceCommon = new Common(config);
            var loginResponse = GetLoginCredential();
            var request = new GetByIdRequest()
            {
                id = id,
                sessionId = loginResponse.Signature
            };

            var response = serviceCommon.GetUsersById(request);
            var listBranches = GetBranches();

            ViewBag.listBranches = listBranches;

            return View(response);
        }



        public List<BranchesUI> GetBranches()
        {
            var config = new SimpleConfiguration()
            {
                apiUrl = GetConfigFromCache("ApiInHouseURL")
            };
            var serviceCommon = new Administration(config);
            var loginResponse = GetLoginCredential();
            var request = new GetBranchesListRequest
            {
                sessionId = loginResponse.Signature,
                pagenumber = 1,
                recordcount = 999
            };

            var response = serviceCommon.GetBranchesList(request);
            var data = JsonConvert.DeserializeObject<List<BranchesUI>>(response.data);

            //return data
            //    .Select(b => b.Code)
            //    .ToList();

            return data;
        }

        public Pagination<DomUsers> ToPaginationUserDto(GenericPagingResponse raw)
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