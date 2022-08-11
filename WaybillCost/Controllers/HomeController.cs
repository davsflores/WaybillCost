using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WaybillCost.Controllers
{ 
    public class HomeController : Controller
    {
        WayBillDbContext _context = new WayBillDbContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WayBill(string sortOrder, string currentFilter, string searchString, int? page)
        {
            //page list
            ViewBag.CurrentSort = sortOrder;
            //sort 
            ViewBag.TypeSortParm = String.IsNullOrEmpty(sortOrder) ? "type_desc" : "";
            ViewBag.WeightSortParm = sortOrder == "weight" ? "weight_desc" : "weight";

            //page list
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;


            var cc = from s in _context.WayBills select s;

            //search
            if (!String.IsNullOrEmpty(searchString))
            {
                cc = cc.Where(s => s.priority.Contains(searchString) || s.type.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "type_desc":
                    cc = cc.OrderByDescending(s => s.type);
                    break;
                case "weight":
                    cc = cc.OrderBy(s => s.weight);
                    break;
                case "weight_desc":
                    cc = cc.OrderByDescending(s => s.weight);
                    break;
                default:
                    cc = cc.OrderBy(s => s.waybillid);
                    break;

                    //var listofData = _context.ComputedCosts.ToList();
                    //return View(listofData);
            }

            return View(cc.ToList());
        }
        public ActionResult Details(int id)
        {
            var data = _context.WayBills.Where(x => x.waybillid == id).FirstOrDefault();
            return View(data);
        }

        [HttpGet]
        public ActionResult Compute()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Compute(WayBill cModel)
        {
            if ((cModel.weight ?? 0) == 0)
            {
                ViewBag.Message = "NO";
                return View();
            }
            else
            {
                var wdata = _context.WeightCosts.Where(x => x.minweight <= cModel.weight).OrderByDescending(x => x.maxweight >= cModel.weight).FirstOrDefault();

                int weightid = wdata.weightid;
                decimal newtotalcost;
                int itemw = (int)cModel.weight;
                newtotalcost = (itemw * (decimal)wdata.cost);

                cModel.priority = wdata.priority;
                cModel.type = wdata.type;
                cModel.cost = wdata.cost;
                cModel.totalcost = newtotalcost;

                _context.WayBills.Add(cModel);
                _context.SaveChanges();
                ViewBag.Message = "YES";
                //return View();
                return RedirectToAction("WayBill");
            }

        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var data = _context.WayBills.Where(x => x.waybillid == id).FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        public ActionResult Edit(WayBill cModel)
        {
            var wdata = _context.WeightCosts.Where(x => x.minweight <= cModel.weight).OrderByDescending(x => x.maxweight >= cModel.weight).FirstOrDefault();

            int weightid = wdata.weightid;
            decimal newtotalcost;
            int itemw = (int)cModel.weight;
            newtotalcost = (itemw * (decimal)wdata.cost);

            var cdata = _context.WayBills.Where(x => x.waybillid == cModel.waybillid).FirstOrDefault();
            if (cdata != null)
            {

                cdata.waybillid = cModel.waybillid;
                cdata.priority = wdata.priority;
                cdata.type = wdata.type;
                cdata.weight = cModel.weight;
                cdata.cost = wdata.cost;
                cdata.totalcost = newtotalcost;
                _context.SaveChanges();

            }

            return RedirectToAction("ComputedCost");

        }


    }
}