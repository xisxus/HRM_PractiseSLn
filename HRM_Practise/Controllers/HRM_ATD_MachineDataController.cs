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


        //Helper date

        public static (bool Success, int Day, int Month, int? Year) ExtractDateComponents(string dateString)
        {
            string[] formats = {
                "d/M/yyyy", "d-M-yyyy", "dd/MM/yyyy", "dd-MM-yyyy",  "d/M/", "d-M-", "dd/MM/", "dd-MM-",
                "M/d/yyyy", "M-d-yyyy", "MM/dd/yyyy", "MM-dd-yyyy",  "M/d/", "M-d-", "MM/dd/", "MM-dd-",
                "d/M", "d-M", "dd/MM", "dd-MM",
                "M/d", "M-d", "MM/dd", "MM-dd",
                "d", "M",
                "yyyy/M/d", "yyyy-M-d", "yyyy/MM/dd", "yyyy-MM-dd",  "yyyy/M/", "yyyy-M-", "yyyy/MM/", "yyyy-MM-",
                "yyyy/MM", "yyyy-MM" 
            };

            if (DateTime.TryParseExact(dateString, formats, System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                int day = parsedDate.Day;
                int month = parsedDate.Month;
                int? year = parsedDate.Year != 1 ? parsedDate.Year : null;

                return (true, day, month, year);
            }

            return (false, 0, 0, null); // Return a failure result
        }







        public IActionResult IndexDateSearch()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetMachineDataWithDate([FromBody] DataTableParameters2 parameters2)
        {
            try
            {
                var draw = parameters2.Draw;
                var start = parameters2.Start;
                var length = parameters2.Length;
                var sortColumn = "";
                var sortColumnDirection = "";

                foreach (var item in parameters2.Order)
                {
                    sortColumnDirection = item.Dir;
                    sortColumn = item.Name;

                }

                var searchValue = parameters2.Search.Value;

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Query
                var query = _context.HRM_ATD_MachineDatas.AsNoTracking().AsQueryable();

                //day month year




                int commonDay = 1;
                int commonMonth = 1;
                int commonYear = 1;



                if (parameters2.StartDate != null)
                {
                   

                    //var firstBatchResults = await tempQuery.ToListAsync();

                    int dateTemp;
                    bool isConversionSuccessful = int.TryParse(parameters2.StartDate, out dateTemp);

                    if (isConversionSuccessful)
                    {
                        if (parameters2.StartDate.Length >= 3)
                        {
                            commonYear = Convert.ToInt16(parameters2.StartDate);
                        }
                        else
                        {
                            commonDay = Convert.ToInt16(parameters2.StartDate);
                            if (dateTemp <= 12)
                            {
                                commonMonth = Convert.ToInt16(parameters2.StartDate);
                            }
                        }
                    }
                    else
                    {
                        var resultDate = ExtractDateComponents(parameters2.StartDate);
                        commonYear = Convert.ToInt16(resultDate.Year);
                        commonMonth = Convert.ToInt16(resultDate.Month);
                        commonDay = Convert.ToInt16(resultDate.Day);
                    }

                    if (commonDay != 1 || commonYear != 1 || commonMonth != 1)
                    {
                        var secondBatchQuery = query;

                        if (commonDay != 1 && commonMonth != 1 && commonYear != 1)
                        {
                            DateTime commonDayDate = new DateTime(commonYear, commonMonth, commonDay);
                            secondBatchQuery = secondBatchQuery.Where(m => m.Date == commonDayDate);
                        }
                        else if (commonDay != 1 || commonMonth != 1)
                        {
                            DateOnly commonDayDate = new DateOnly(commonYear, commonMonth, commonDay);

                            if (commonDayDate.Month != 1)
                            {
                                secondBatchQuery = secondBatchQuery.Where(m => m.Date.Month == commonDayDate.Month || m.Date.Day == commonDayDate.Day);
                            }
                            else
                            {
                                secondBatchQuery = secondBatchQuery.Where(m => m.Date.Day == commonDayDate.Day);
                            }
                        }
                        else if (commonYear != 1)
                        {
                            DateOnly commonDayDate = new DateOnly(commonYear, commonMonth, commonDay);
                            var result = Convert.ToString(commonYear);

                            if (result.Length == 3)
                            {
                                secondBatchQuery = _context.HRM_ATD_MachineDatas.AsNoTracking().Where(item => item.Date.Year.ToString().StartsWith(result));
                            }
                            else
                            {
                                secondBatchQuery = secondBatchQuery.Where(m => m.Date.Year == commonDayDate.Year);
                            }
                        }

                        var tempQuery = query;

                        tempQuery = tempQuery.Where(m =>
                           (m.FingerPrintId != null && m.FingerPrintId.Contains(parameters2.StartDate))) ;
                           //||
                           //(m.MachineId != null && m.MachineId.Contains(parameters2.StartDate)) ||
                           //(m.HOALR != null && m.HOALR.Contains(parameters2.StartDate)));

                        var chkPoint = await tempQuery.CountAsync();

                        query = tempQuery.Concat(secondBatchQuery);


                        var chkPoint2 = await query.CountAsync();

                       
                    }
                    else
                    {
                        
                    }


                }












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
                    draw = "0",
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<object>(),
                    error = "An error occurred while processing your request."



                });
            }
        }






        public IActionResult Index56Procedure()
        {
            return View();
        }

        public async Task<JsonResult> GetMachineDataFromProcedure([FromBody] DataTableParameters parameters)
        {
            try
            {
                // Define output parameters
                int totalRecords = 0;
                int filteredRecords = 0;

                // Create a list to hold the results
                var data = new List<object>();

                // Define the connection string (make sure to replace with your actual connection string)
                var connectionString = _context.Database.GetDbConnection().ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("GetMachineData", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        command.Parameters.AddWithValue("@StartDate", parameters.StartDate.HasValue ? (object)parameters.StartDate.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@EndDate", parameters.EndDate.HasValue ? (object)parameters.EndDate.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@SearchValue", string.IsNullOrEmpty(parameters.Search.Value) ? (object)DBNull.Value : parameters.Search.Value);
                        command.Parameters.AddWithValue("@SortColumn", parameters.Order.Count > 0 ? parameters.Order[0].Name : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SortDirection", parameters.Order.Count > 0 ? parameters.Order[0].Dir : "ASC");
                        command.Parameters.AddWithValue("@Start", parameters.Start);
                        command.Parameters.AddWithValue("@Length", parameters.Length);

                        // Output parameters
                        var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        var filteredRecordsParam = new SqlParameter("@FilteredRecords", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        command.Parameters.Add(totalRecordsParam);
                        command.Parameters.Add(filteredRecordsParam);

                        // Execute the command
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                data.Add(new
                                {
                                    AutoId = reader["AutoId"],
                                    FingerPrintId = reader["FingerPrintId"],
                                    MachineId = reader["MachineId"],
                                    Date = ((DateTime)reader["Date"]).ToString("yyyy-MM-dd"),
                                    Time = ((TimeSpan)reader["Time"]).ToString(@"hh\:mm\:ss"),
                                    Latitude = reader["Latitude"],
                                    Longitude = reader["Longitude"],
                                    HOALR = reader["HOALR"]
                                });
                            }
                        }

                        // Get output parameter values
                        totalRecords = (int)totalRecordsParam.Value;
                        filteredRecords = (int)filteredRecordsParam.Value;
                    }
                }

                return Json(new
                {
                    recordsFiltered = filteredRecords,
                    recordsTotal = totalRecords,
                    data = data
                });
            }
            catch (Exception ex)
            {
                // Log the exception here (consider using a logging framework)
                return Json(new
                {
                    draw = "0",
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<object>(),
                    error = "An error occurred while processing your request."
                });
            }
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
