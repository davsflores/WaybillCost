using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WaybillCost.Models;


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

        public ActionResult ViewMultipleTable(string sortOrder, string currentFilter, string searchString, int? page)
        {

            // WayBillDbContext sd = new WayBillDbContext();
            List<WayBill> WayBillList = _context.WayBills.ToList();
            List<Origin> OriginsList = _context.Origins.ToList();
            List<Destination> DestinationsList = _context.Destinations.ToList();

            var cc = from wl in WayBillList
                     join ol in OriginsList on wl.waybillid equals ol.waybillid into table1
                     from ol in table1.DefaultIfEmpty()
                     join dl in DestinationsList on wl.waybillid equals dl.waybillid into table2
                     from dl in table2.DefaultIfEmpty()
                     select new WaybillOriginDestinationModel { waybillDetails = wl, originDetails = ol, destinationDetails = dl };


            //page list
            ViewBag.CurrentSort = sortOrder;
            //sort 
            ViewBag.TypeSortParm = String.IsNullOrEmpty(sortOrder) ? "type_desc" : "";
            ViewBag.DateSortParm = sortOrder == "datecreated" ? "datecreated_desc" : "datecreated";
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


            //var cc = from s in _context.WayBills select s;

            //search
            if (!String.IsNullOrEmpty(searchString))
            {
                cc = cc.Where(s => s.waybillDetails.priority.Contains(searchString) || s.waybillDetails.type.Contains(searchString));
            }

            //sort
            switch (sortOrder)
            {
                case "type_desc":
                    cc = cc.OrderByDescending(s => s.waybillDetails.type);
                    break;
                case "datecreated":
                    cc = cc.OrderBy(s => s.waybillDetails.weight);
                    break;
                case "datecreated_desc":
                    cc = cc.OrderByDescending(s => s.waybillDetails.weight);
                    break;
                case "weight":
                    cc = cc.OrderBy(s => s.waybillDetails.weight);
                    break;
                case "weight_desc":
                    cc = cc.OrderByDescending(s => s.waybillDetails.weight);
                    break;
                default:
                    cc = cc.OrderBy(s => s.waybillDetails.waybillid);
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
        public ActionResult Compute(WayBill cModel, WaybillOriginDestinationModel wodModel)
        {
            if ((wodModel.waybillDetails.weight ?? 0) == 0)
            {
                ViewBag.Message = "NO";
                return View();
            }
            else
            {
                var wdata = _context.WeightCosts.Where(x => x.minweight <= wodModel.waybillDetails.weight).OrderByDescending(x => x.maxweight >= wodModel.waybillDetails.weight).FirstOrDefault();

                int weightid = wdata.weightid;
                decimal newtotalcost;
                int itemw = (int)wodModel.waybillDetails.weight;
                newtotalcost = (itemw * (decimal)wdata.cost);

                cModel.weight = wodModel.waybillDetails.weight;
                cModel.priority = wdata.priority;
                cModel.type = wdata.type;
                cModel.cost = wdata.cost;
                cModel.totalcost = newtotalcost;
                cModel.datecreated = DateTime.Now;

                _context.WayBills.Add(cModel);
                _context.SaveChanges();

                wodModel.originDetails.waybillid = cModel.waybillid;

                _context.Origins.Add(wodModel.originDetails);
                _context.SaveChanges();

                wodModel.destinationDetails.waybillid = cModel.waybillid;

                _context.Destinations.Add(wodModel.destinationDetails);
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