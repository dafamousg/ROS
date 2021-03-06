﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ROS.Web.Data;
using ROS.Web.Models;
using ROS.Web.Models.ClubViewModels;

namespace ROS.Web.Controllers
{
    [Authorize]
    public class ClubsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClubsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: Clubs
        public async Task<IActionResult> Index(string sortOrder)
        {
            var clubs = await _context.Clubs.Include(c => c.Owner)
                .Include(u => u.ClubUsers).Include(a => a.Applications).ToListAsync();

            var clubVMList = new List<GetClubsViewModel>();

            var currentUser = GetCurrentUser().Id;

            foreach (var item in clubs)
            {
                clubVMList.Add(new GetClubsViewModel
                {
                    ClubId = item.Id,
                    ClubName = item.Name,
                    FoundedDate = item.FoundedDate,
                    IsActive = item.IsActive,
                    NumberOfMembers = item.ClubUsers.Count,
                    Owner = item.Owner,
                    IsMember = item.ClubUsers.Exists(m => m.UserId == currentUser),
                    HasApplied = item.Applications.Exists(a => (a.UserId == currentUser && a.Status == ApplicationStatus.Pending))
                });
            }

            ViewData["NameSortParm"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["MemberSortParm"] = sortOrder == "member" ? "member_desc" : "member";

            var sortQuery = from x in clubVMList
                            select x;

            switch (sortOrder)
            {
                case "name_desc":
                    sortQuery = sortQuery.OrderByDescending(s => s.ClubName);
                    break;

                case "name":
                    sortQuery = sortQuery.OrderBy(s => s.ClubName);
                    break;

                case "date":
                    sortQuery = sortQuery.OrderBy(s => s.FoundedDate);
                    break;

                case "date_desc":
                    sortQuery = sortQuery.OrderByDescending(s => s.FoundedDate);
                    break;

                case "member":
                    sortQuery = sortQuery.OrderBy(s => s.NumberOfMembers);
                    break;

                case "member_desc":
                    sortQuery = sortQuery.OrderByDescending(s => s.NumberOfMembers);
                    break;

                default:
                    break;
            }

            return View(sortQuery);
        }

        // GET: Clubs/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs
                .Include(o => o.Owner)
                .Include(u => u.ClubUsers)
                .Include(a => a.Applications)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (club == null)
            {
                return NotFound();
            }

            var clubViewModel = new ClubDetailsViewModel
            {
                ClubId = club.Id,
                Name = club.Name,
                IsActive = club.IsActive,
                Members = club.ClubUsers,
                NumberOfMembers = club.ClubUsers.Count(),
                FoundedDate = club.FoundedDate,
                JoinedDate = club.JoinedDate,
                Owner = club.Owner,
                PendingApplications = club.Applications.Where(a => a.Status == ApplicationStatus.Pending).ToList()
            };

            int numberOfApplicants = 0;

            if (clubViewModel.PendingApplications != null)
            {
                numberOfApplicants = clubViewModel.PendingApplications.Count();
            }

            ViewData["NumberOfApplicants"] = numberOfApplicants;

            return View(clubViewModel);
        }

        //GET: Clubs/ClubMembers
        [HttpGet]
        public async Task<IActionResult> ClubMembers(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs
                .Include(o => o.Owner)
                .Include(u => u.ClubUsers)
                .ThenInclude(u => u.User)
                .Include(a => a.Applications)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (club == null)
            {
                return NotFound();
            }

            var clubViewModel = new ClubDetailsViewModel
            {
                ClubId = club.Id,
                Name = club.Name,
                IsActive = club.IsActive,
                Members = club.ClubUsers,
                NumberOfMembers = club.ClubUsers.Count(),
                FoundedDate = club.FoundedDate,
                JoinedDate = club.JoinedDate,
                Owner = club.Owner,
                PendingApplications = club.Applications.Where(a => a.Status == ApplicationStatus.Pending).ToList()
            };

            int numberOfApplicants = 0;

            if (clubViewModel.PendingApplications != null)
            {
                numberOfApplicants = clubViewModel.PendingApplications.Count();
            }

            ViewData["NumberOfApplicants"] = numberOfApplicants;

            return View(clubViewModel);
        }

        //GET: Clubs/Details/ClubApplications/id
        [HttpGet]
        public async Task<IActionResult> ClubApplications(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs
                .Include(o => o.Owner)
                .Include(c => c.ClubUsers)
                .Include(a => a.Applications)
                .ThenInclude(k => k.User)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (club == null)
            {
                return NotFound();
            }

            var clubViewModel = new ClubDetailsViewModel
            {
                ClubId = club.Id,
                Name = club.Name,
                IsActive = club.IsActive,
                Members = club.ClubUsers,
                FoundedDate = club.FoundedDate,
                JoinedDate = club.JoinedDate,
                Owner = club.Owner,
                PendingApplications = club.Applications.Where(a => a.Status == ApplicationStatus.Pending).ToList()
            };

            int numberOfApplicants = 0;

            if (clubViewModel.PendingApplications != null)
            {
                numberOfApplicants = clubViewModel.PendingApplications.Count();
            }

            ViewData["NumberOfApplicants"] = numberOfApplicants;

            return View(clubViewModel);
        }

        [Authorize]
        // GET: MyClubs
        public async Task<IActionResult> MyClubs(string sortOrder)
        {
            string _userId = GetCurrentUser().Id;

            var clubs = await _context.Clubs
                .Include(o => o.Owner)
                .Include(u => u.ClubUsers)
                .ToListAsync();

            var clubVMList = new List<GetClubsViewModel>();

            foreach (var item in clubs)
            {
                if(item.ClubUsers.Exists(m => m.UserId == _userId))
                {
                    clubVMList.Add(new GetClubsViewModel
                    {
                        ClubId = item.Id,
                        ClubName = item.Name,
                        FoundedDate = item.FoundedDate,
                        IsActive = item.IsActive,
                        IsMember = item.ClubUsers.Exists(m => m.UserId == _userId),
                        NumberOfMembers = item.ClubUsers.Count,
                        Owner = item.Owner
                    });
                }
            }

            if (clubVMList == null || clubVMList.Count() <= 0)
            {
                return RedirectToAction(nameof(Create));
            }

            ViewData["NameSortParm"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["MemberSortParm"] = sortOrder == "member" ? "member_desc" : "member";

            var clubSort = from x in clubVMList
                           select x;

            switch (sortOrder)
            {
                case "name_desc":
                    clubSort = clubSort.OrderByDescending(s => s.ClubName);
                    break;

                case "name":
                    clubSort = clubSort.OrderBy(s => s.ClubName);
                    break;

                case "date":
                    clubSort = clubSort.OrderBy(s => s.FoundedDate);
                    break;

                case "date_desc":
                    clubSort = clubSort.OrderByDescending(s => s.FoundedDate);
                    break;

                case "member":
                    clubSort = clubSort.OrderBy(s => s.NumberOfMembers);
                    break;

                case "member_desc":
                    clubSort = clubSort.OrderByDescending(s => s.NumberOfMembers);
                    break;

                default:
                    break;
            }

            return View(clubSort);
        }

        // GET: Clubs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clubs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,FoundedDate,JoinedDate,IsActive,Id")] Club club)
        {
            if (ModelState.IsValid)
            {
                club.Owner = _context.Users.FirstOrDefault(u => u.UserName == HttpContext.User.Identity.Name);

                // Adds "Club Admin"-role to the user
                var user = _context.Users.FirstOrDefault(o => o.UserName == club.Owner.UserName);
                await _userManager.AddToRoleAsync(user, "Club Admin");

                club.JoinedDate = DateTime.Now;
                club.IsActive = true;
                club.Id = Guid.NewGuid();

                // adds user to club user table
                var clubUser = new ClubUser { User = user, ClubId = club.Id, Club = club, UserId = club.Owner.Id, Id = Guid.NewGuid() };
                _context.Add(clubUser);

                _context.Add(club);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(club);
        }

        // GET: Clubs/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs.Include(o => o.Owner).SingleOrDefaultAsync(m => m.Id == id);
            if (club == null)
            {
                return NotFound();
            }

            string _userId = GetCurrentUser().Id;

            if (club.Owner.Id != _userId)
            {
                return Forbid();
            }

            return View(club);
        }

        // POST: Clubs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,FoundedDate,JoinedDate,IsActive,Id")] Club club)
        {
            if (id != club.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(club);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClubExists(club.Id))
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
            return View(club);
        }

        // GET: Clubs/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs.Include(o => o.Owner)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (club == null)
            {
                return NotFound();
            }

            string _userId = GetCurrentUser().Id;

            if (club.Owner.Id != _userId)
            {
                return Forbid();
            }

            return View(club);
        }

        // POST: Clubs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var club = await _context.Clubs.SingleOrDefaultAsync(m => m.Id == id);
            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clubs/Apply/Id
        [HttpGet]
        public async Task<IActionResult> Apply(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs.SingleOrDefaultAsync(m => m.Id == id);

            if (club == null)
            {
                return NotFound();
            }

            return View(club);
        }

        //POST: Clubs/Apply/id
        [HttpPost, ActionName("Apply")]
        public async Task<IActionResult> ApplyConfirmed(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var club = await _context.Clubs.Include(a => a.Applications).SingleOrDefaultAsync(m => m.Id == id);

            var application = new ClubApplication
            {
                ClubId = club.Id,
                Date = DateTime.Now,
                UserId = GetCurrentUser().Id,
                Status = ApplicationStatus.Pending,
                Id = Guid.NewGuid(),
                Club = club,
                User = GetCurrentUser()
            };

            club.Applications.Add(application);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ApproveApplication(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.ClubApplications
                .Include(c => c.User)
                .Include(c => c.Club)
                .ThenInclude(c => c.ClubUsers)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            application.Status = ApplicationStatus.Approved;

            var user = application.User;
            var club = application.Club;

            club.ClubUsers.Add(new ClubUser { Id = Guid.NewGuid(), Club = club, User = user });
            _context.SaveChanges();

            return RedirectToAction("ClubApplications", club);
        }

        public async Task<IActionResult> RejectApplication(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.ClubApplications.SingleOrDefaultAsync(c => c.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            var getClub = await _context.Clubs.FirstOrDefaultAsync(o => o.Id == application.ClubId);

            application.Status = ApplicationStatus.Rejected;
            _context.SaveChanges();

            return RedirectToAction("ClubApplications", getClub);
        }

        //POST: KickMember from Club
        public async Task<IActionResult> KickMember(Guid? clubUserId, Guid? clubId)
        {
            if (clubUserId == null || clubId == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs.Include(c => c.ClubUsers).SingleOrDefaultAsync(o => o.Id == clubId);

            if (club == null)
            {
                return NotFound();
            }

            var getUser = club.ClubUsers.SingleOrDefault(u => u.Id == clubUserId);

            if (getUser == null)
            {
                return NotFound();
            }

            club.ClubUsers.Remove(getUser);
            _context.Clubs.Update(club);
            await _context.SaveChangesAsync();

            return RedirectToAction("ClubMembers", club);

        }

        private bool ClubExists(Guid id)
        {
            return _context.Clubs.Any(e => e.Id == id);
        }


        #region Helpers
        public ApplicationUser GetCurrentUser()
        {
            return _context.Users.SingleOrDefault(u => u.UserName == HttpContext.User.Identity.Name);
        }
        #endregion

    }
}
