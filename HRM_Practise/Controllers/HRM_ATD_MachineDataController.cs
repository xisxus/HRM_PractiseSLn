
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HRM_Practise.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HRM_Practise.Controllers
{
    public class HRM_ATD_MachineDataController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public HRM_ATD_MachineDataController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }


        [HttpPost]
        public IActionResult GetPaginatedData(int draw, int start, int length)
        {
            // Replace with your actual data source
            var allData = _context.HRM_ATD_MachineDatas.Take(1000);

            // Filtering, sorting, or searching logic (if any)
            var filteredData = allData;

            // Pagination
            var paginatedData = filteredData
                .Skip(start)
                .Take(length)
                .ToList();

            return Json(new
            {
                draw = draw,
                recordsTotal = allData.Count(),
                recordsFiltered = filteredData.Count(),
                data = paginatedData
            });
        }


       



        public IActionResult Index56()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetMachineData([FromBody] DataTableParameters parameters)
       {
            try
            {
               

                var draw = parameters.Draw;
                var start = parameters.Start;
                var length = parameters.Length;
                var sortColumn = ""; 
                var sortColumnDirection = ""; 

                foreach (var item in parameters.Order)
                {
                     sortColumnDirection  = item.Dir;
                    sortColumn = item.Name;
                    
                }

                var searchValue = parameters.Search.Value; 

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Query
                var query = _context.HRM_ATD_MachineDatas.AsNoTracking().AsQueryable();

                if (parameters.StartDate.HasValue)
                {
                    query = query.Where(m => m.Date == parameters.StartDate.Value);
                }

                //if (parameters.EndDate.HasValue)
                //{
                //    query = query.Where(m => m.Date <= parameters.EndDate.Value);
                //}


                // Get total count
                recordsTotal = await query.CountAsync();

                // Apply Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        (m.FingerPrintId != null && m.FingerPrintId.Contains(searchValue)) ||
                        (m.MachineId != null && m.MachineId.Contains(searchValue)) ||
                        (m.HOALR != null && m.HOALR.Contains(searchValue)));
                }

                // Get filtered count
                var recordsFiltered = await query.CountAsync();

                // Apply Sorting
                if ((!string.IsNullOrEmpty(sortColumn)) && (!string.IsNullOrEmpty(sortColumnDirection)))
                {
                    query = sortColumn.ToLower() switch
                    {
                        "fingerprint" => sortColumnDirection.ToLower() == "asc"
                            ? query.OrderBy(m => m.FingerPrintId)
                            : query.OrderByDescending(m => m.FingerPrintId),
                        "machineid" => sortColumnDirection.ToLower() == "asc"
                            ? query.OrderBy(m => m.MachineId)
                            : query.OrderByDescending(m => m.MachineId),
                        "date" => sortColumnDirection.ToLower() == "asc"
                            ? query.OrderBy(m => m.Date)
                            : query.OrderByDescending(m => m.Date),
                        _ => sortColumnDirection.ToLower() == "asc"
                            ? query.OrderBy(m => m.AutoId)
                            : query.OrderByDescending(m => m.AutoId)
                    };
                }

                // Apply Pagination
                var data = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(m => new
                    {
                        m.AutoId,
                        m.FingerPrintId,
                        m.MachineId,
                        Date = m.Date.ToString("yyyy-MM-dd"),
                        Time = m.Time.ToString("HH:mm:ss"),
                        m.Latitude,
                        m.Longitude,
                        m.HOALR
                    })
                    .ToListAsync();

                return Json(new
                {
                   // draw = draw,
                    recordsFiltered = recordsFiltered,
                    recordsTotal = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return Json(new
                {
                    draw =  "0" ,
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<object>(),
                    error = "An error occurred while processing your request."

                  

            });
            }
        }







        public IActionResult Index(int pageNumber = 1, int pageSize = 10, string searchValue = "")
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SP_GetHRMMachineData";
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                var parameters = new[]
                {
            new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber },
            new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
            new SqlParameter("@SearchValue", SqlDbType.NVarChar, 100) { Value = (object)searchValue ?? DBNull.Value },
            new SqlParameter("@TotalRecords", SqlDbType.Int) { Direction = ParameterDirection.Output }
        };

                command.Parameters.AddRange(parameters);

                // Ensure connection is open
                _context.Database.OpenConnection();

                // Get data
                var data = new List<HRM_ATD_MachineData>();
                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        data.Add(new HRM_ATD_MachineData
                        {
                            // Map your properties here
                            FingerPrintId = result["FingerPrintId"].ToString(),
                            MachineId = result["MachineId"].ToString(),
                            Date = Convert.ToDateTime(result["Date"]),
                            // Add other properties as needed
                        });
                    }
                }

                // Get total records from output parameter
                int totalRecords = Convert.ToInt32(((SqlParameter)command.Parameters["@TotalRecords"]).Value);

                var model = new PaginatedResult<HRM_ATD_MachineData>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                ViewData["SearchValue"] = searchValue;
                return View(model);
            }
        }




        // GET: HRM_ATD_MachineData
        //public IActionResult Index(int pageNumber = 1, int pageSize = 10, string searchValue = "")
        //{
        //    // Base query
        //    var query = _context.HRM_ATD_MachineDatas.AsQueryable();

        //    // Apply search filter
        //    if (!string.IsNullOrEmpty(searchValue))
        //    {
        //        query = query.Where(d =>
        //            d.FingerPrintId.Contains(searchValue) ||
        //            d.MachineId.Contains(searchValue));
        //    }

        //    // Get total record count
        //    var totalRecords = query.Count();

        //    // Apply pagination
        //    var data = query
        //        .OrderBy(d => d.Date) // Adjust ordering as needed
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToList();

        //    // Pass data to the view
        //    var model = new PaginatedResult<HRM_ATD_MachineData>
        //    {
        //        Data = data,
        //        TotalRecords = totalRecords,
        //        PageNumber = pageNumber,
        //        PageSize = pageSize
        //    };

        //    ViewData["SearchValue"] = searchValue; // Preserve search value
        //    return View(model);
        //}

        // GET: HRM_ATD_MachineData/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hRM_ATD_MachineData = await _context.HRM_ATD_MachineDatas
                .FirstOrDefaultAsync(m => m.AutoId == id);
            if (hRM_ATD_MachineData == null)
            {
                return NotFound();
            }

            return View(hRM_ATD_MachineData);
        }

        // GET: HRM_ATD_MachineData/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HRM_ATD_MachineData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AutoId,FingerPrintId,MachineId,Date,Time,Latitude,Longitude,HOALR")] HRM_ATD_MachineData hRM_ATD_MachineData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hRM_ATD_MachineData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hRM_ATD_MachineData);
        }

        // GET: HRM_ATD_MachineData/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hRM_ATD_MachineData = await _context.HRM_ATD_MachineDatas.FindAsync(id);
            if (hRM_ATD_MachineData == null)
            {
                return NotFound();
            }
            return View(hRM_ATD_MachineData);
        }

        // POST: HRM_ATD_MachineData/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AutoId,FingerPrintId,MachineId,Date,Time,Latitude,Longitude,HOALR")] HRM_ATD_MachineData hRM_ATD_MachineData)
        {
            if (id != hRM_ATD_MachineData.AutoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hRM_ATD_MachineData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HRM_ATD_MachineDataExists(hRM_ATD_MachineData.AutoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hRM_ATD_MachineData);
        }

        // GET: HRM_ATD_MachineData/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hRM_ATD_MachineData = await _context.HRM_ATD_MachineDatas
                .FirstOrDefaultAsync(m => m.AutoId == id);
            if (hRM_ATD_MachineData == null)
            {
                return NotFound();
            }

            return View(hRM_ATD_MachineData);
        }

        // POST: HRM_ATD_MachineData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hRM_ATD_MachineData = await _context.HRM_ATD_MachineDatas.FindAsync(id);
            if (hRM_ATD_MachineData != null)
            {
                _context.HRM_ATD_MachineDatas.Remove(hRM_ATD_MachineData);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HRM_ATD_MachineDataExists(int id)
        {
            return _context.HRM_ATD_MachineDatas.Any(e => e.AutoId == id);
        }
    }
}
