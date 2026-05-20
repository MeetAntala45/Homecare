using System.Text.Json;
using System.Text.RegularExpressions;
using Homecare.Application.Constants;
using Homecare.Application.DTOs.ServicePartner;
using Homecare.Application.Interfaces.ServicePartner;
using Homecare.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PartnerEntity = Homecare.Domain.Entities.ServicePartner;
using PartnerEducationEntity = Homecare.Domain.Entities.PartnerEducation;
using PartnerExperienceEntity = Homecare.Domain.Entities.PartnerExperience;
using PartnerSkillEntity = Homecare.Domain.Entities.PartnerSkill;
using PartnerServiceOfferedEntity = Homecare.Domain.Entities.PartnerServiceOffered;
using PartnerLanguageEntity = Homecare.Domain.Entities.PartnerLanguage;
using PartnerDocumentEntity = Homecare.Domain.Entities.PartnerDocument;
using DropdownOptionDto = Homecare.Application.DTOs.ServicePartner.DropdownOptionDto;
using Homecare.Application.Constants.Pagination;
using Homecare.Domain.Enums;
using Homecare.Application.Common.Models;
using Homecare.Application.Interfaces;

namespace Homecare.Application.Services.ServicePartner;

public class ServicePartnerService : IServicePartnerService
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinary;
    private readonly ICurrentUserService _currentUser;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> AllowedMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf", "image/jpeg", "image/jpg", "image/png"
        };

    public ServicePartnerService(
        AppDbContext context,
        ICloudinaryService cloudinary,
        ICurrentUserService currentUser)
    {
        _context = context;
        _cloudinary = cloudinary;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<ServicePartnerResponseDto>> CreateServicePartnerAsync(
        CreateServicePartnerRequestDto dto)
    {
        var personal = Deserialize<PersonalDetailDto>(dto.PersonalDetail) ?? new();
        var educations = Deserialize<List<EducationInfoDto>>(dto.EducationInfoList) ?? new();
        var professionals = Deserialize<List<ProfessionalInfoDto>>(dto.ProfessionalInfoList) ?? new();
        var skills = Deserialize<List<SkillExpertiseDto>>(dto.SkillExpertiseList) ?? new();
        var languages = Deserialize<List<LanguageDto>>(dto.LanguageList) ?? new();

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(personal.FullName))
            errors.Add("Full name is required.");

        if (!DateTime.TryParse(personal.DateOfBirth, out var dateOfBirth))
            errors.Add("Valid date of birth is required.");

        if (string.IsNullOrWhiteSpace(personal.Email))
            errors.Add("Email is required.");
        else if (!EmailRegex.IsMatch(personal.Email))
            errors.Add("Email format is invalid.");

        if (!int.TryParse(personal.ApplyingFor, out var serviceTypeId) || serviceTypeId <= 0)
            errors.Add("Service type (Applying For) is required.");

        var validEducations = educations
            .Where(e => !string.IsNullOrWhiteSpace(e.SchoolCollege)
                     || !string.IsNullOrWhiteSpace(e.PassingYear)
                     || e.Marks != null)
            .ToList();

        if (!validEducations.Any())
            errors.Add("At least one educational qualification is required.");

        var currentYear = (short)DateTime.UtcNow.Year;

        foreach (var edu in validEducations)
        {
            if (string.IsNullOrWhiteSpace(edu.SchoolCollege))
                errors.Add("School/College name is required for all education entries.");

            if (string.IsNullOrWhiteSpace(edu.PassingYear))
                errors.Add($"Passing year is required for '{edu.SchoolCollege}'.");
            else if (!short.TryParse(edu.PassingYear, out var year))
                errors.Add($"Invalid passing year: '{edu.PassingYear}'.");
            else if (year >= currentYear)
                errors.Add($"Passing year '{year}' must be less than current year ({currentYear}).");

            if (edu.Marks == null)
                errors.Add($"Marks are required for '{edu.SchoolCollege}'.");
        }

        var duplicateEducations = validEducations
            .GroupBy(e => new
            {
                School = e.SchoolCollege?.Trim().ToLower(),
                Year = e.PassingYear?.Trim()
            })
            .Where(g => g.Count() > 1)
            .Select(g => g.Key.School)
            .ToList();
        if (duplicateEducations.Any())
            errors.Add($"Duplicate education entries: {string.Join(", ", duplicateEducations)}.");

        var validProfessionals = professionals
            .Where(e => !string.IsNullOrWhiteSpace(e.CompanyName)
                     || !string.IsNullOrWhiteSpace(e.Role)
                     || !string.IsNullOrWhiteSpace(e.FromDate)
                     || !string.IsNullOrWhiteSpace(e.ToDate))
            .ToList();

        foreach (var exp in validProfessionals)
        {
            if (string.IsNullOrWhiteSpace(exp.CompanyName))
                errors.Add("Company name is required for all professional entries.");

            if (string.IsNullOrWhiteSpace(exp.Role))
                errors.Add("Role is required for all professional entries.");

            if (string.IsNullOrWhiteSpace(exp.FromDate))
                errors.Add($"From date is required for '{exp.CompanyName}'.");
            else if (!DateTime.TryParse(exp.FromDate, out var fromDate))
                errors.Add($"Invalid from date for '{exp.CompanyName}'.");
            else if (!string.IsNullOrWhiteSpace(exp.ToDate))
            {
                if (!DateTime.TryParse(exp.ToDate, out var toDate))
                    errors.Add($"Invalid to date for '{exp.CompanyName}'.");
                else if (toDate <= fromDate)
                    errors.Add($"To date must be after from date for '{exp.CompanyName}'.");
            }
        }

        var duplicateProfessionals = validProfessionals
            .GroupBy(e => new
            {
                Company = e.CompanyName?.Trim().ToLower(),
                Role = e.Role?.Trim().ToLower()
            })
            .Where(g => g.Count() > 1)
            .Select(g => g.Key.Company)
            .ToList();
        if (duplicateProfessionals.Any())
            errors.Add($"Duplicate professional entries: {string.Join(", ", duplicateProfessionals)}.");

        if (!skills.Any())
            errors.Add("At least one skill category is required.");

        var allSubCategoryIds = skills
            .SelectMany(s => s.SubCategories)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => int.TryParse(id, out var n) ? n : 0)
            .Where(n => n > 0)
            .Distinct()
            .ToList();

        if (!allSubCategoryIds.Any())
            errors.Add("At least one service is required.");

        if (!languages.Any())
            errors.Add("At least one language is required.");

        var parsedLanguages = new List<(Language lang, Proficiency prof)>();
        foreach (var l in languages)
        {
            if (string.IsNullOrWhiteSpace(l.Language))
            {
                errors.Add("Language is required for all language entries.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(l.Proficiency))
            {
                errors.Add("Proficiency is required for all language entries.");
                continue;
            }
            Enum.TryParse(l.Language.Trim(), ignoreCase: true, out Language langEnum);
            Enum.TryParse(l.Proficiency.Trim(), ignoreCase: true, out Proficiency profEnum);
            parsedLanguages.Add((langEnum, profEnum));
        }

        var duplicateLanguages = parsedLanguages
            .GroupBy(x => x.lang)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key.ToString())
            .ToList();
        if (duplicateLanguages.Any())
            errors.Add($"Duplicate language entries: {string.Join(", ", duplicateLanguages)}.");

        if (!dto.AttachmentFiles.Any())
            errors.Add("At least one document upload is required.");

        foreach (var file in dto.AttachmentFiles)
        {
            if (!AllowedMimeTypes.Contains(file.ContentType))
                errors.Add($"'{file.FileName}': only PDF, JPG, and PNG are allowed.");
            if (file.Length > 20 * 1024 * 1024)
                errors.Add($"'{file.FileName}': file size must not exceed 20 MB.");
        }

        if (errors.Any())
            return ApiResponse<ServicePartnerResponseDto>.Fail(errors.First());

        var normalizedEmail = personal.Email.Trim().ToLower();

        if (await _context.ServicePartners.AnyAsync(p => p.Email == normalizedEmail))
            return ApiResponse<ServicePartnerResponseDto>.Fail(
                "A service partner with this email already exists.");

        if (await _context.ServicePartners.AnyAsync(p => p.MobileNumber == personal.MobileNumber))
            return ApiResponse<ServicePartnerResponseDto>.Fail(
                "A service partner with this mobile number already exists.");

        Enum.TryParse(personal.Gender?.Trim(), ignoreCase: true, out Gender gender);

        var partner = new PartnerEntity
        {
            FullName = personal.FullName.Trim().ToTitleCase(),
            Email = normalizedEmail,
            MobileNumber = personal.MobileNumber,
            DateOfBirth = DateTime.SpecifyKind(dateOfBirth, DateTimeKind.Utc),
            Gender = gender,
            ServiceTypeId = serviceTypeId,
            PermanentAddress = personal.PermanentAddress?.Trim() ?? string.Empty,
            ResidentialAddress = personal.ResidentialAddress?.Trim() ?? string.Empty,
            Status = PartnerStatus.Pending,
        };
        partner.SetCreated(0);

        if (dto.ProfileImage is { Length: > 0 })
            partner.ProfileImage = await SaveFileAsync(dto.ProfileImage, "partner-profiles");

        foreach (var edu in validEducations)
        {
            short.TryParse(edu.PassingYear, out var year);
            var entry = new PartnerEducationEntity
            {
                InstituteName = edu.SchoolCollege.Trim(),
                PassingYear = year,
                MarksPercentage = edu.Marks,
            };
            entry.SetCreated(0);
            partner.Educations.Add(entry);
        }

        foreach (var exp in validProfessionals
                     .Where(e => !string.IsNullOrWhiteSpace(e.CompanyName)
                              && !string.IsNullOrWhiteSpace(e.Role)))
        {
            DateTime.TryParse(exp.FromDate, out var fromDate);
            DateTime? toDate = DateTime.TryParse(exp.ToDate, out var td) ? td : null;

            var entry = new PartnerExperienceEntity
            {
                CompanyName = exp.CompanyName!.Trim(),
                Role = exp.Role!.Trim(),
                FromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc),
                ToDate = toDate.HasValue
                    ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc)
                    : null,
            };
            entry.SetCreated(0);
            partner.Experiences.Add(entry);
        }

        var distinctCategoryIds = skills
            .Where(s => int.TryParse(s.CategoryId, out _))
            .Select(s => int.Parse(s.CategoryId))
            .Distinct()
            .ToList();

        foreach (var categoryId in distinctCategoryIds)
        {
            var entry = new PartnerSkillEntity { CategoryId = categoryId };
            entry.SetCreated(0);
            partner.Skills.Add(entry);
        }

        foreach (var subCategoryId in allSubCategoryIds)
        {
            var entry = new PartnerServiceOfferedEntity { SubCategoryId = subCategoryId };
            entry.SetCreated(0);
            partner.ServicesOffered.Add(entry);
        }

        foreach (var (lang, prof) in parsedLanguages)
        {
            var entry = new PartnerLanguageEntity { Language = lang, Proficiency = prof };
            entry.SetCreated(0);
            partner.Languages.Add(entry);
        }

        _context.ServicePartners.Add(partner);
        await _context.SaveChangesAsync();

        foreach (var file in dto.AttachmentFiles)
        {
            var filePath = await SaveFileAsync(file, "partner-documents");
            var docEntry = new PartnerDocumentEntity
            {
                PartnerId = partner.Id,
                DocumentName = Path.GetFileNameWithoutExtension(file.FileName),
                FilePath = filePath,
                FileSizeKb = (int)(file.Length / 1024),
                FileType = Path.GetExtension(file.FileName).TrimStart('.').ToLower(),
            };
            docEntry.SetCreated(0);
            _context.PartnerDocuments.Add(docEntry);
        }

        await _context.SaveChangesAsync();

        return ApiResponse<ServicePartnerResponseDto>.SuccessResponse(
            "Application submitted successfully.", new ServicePartnerResponseDto
            {
                Id = partner.Id,
                ServicePartnerId = $"{partner.Id:D3}",
                FullName = partner.FullName,
                Email = partner.Email,
                MobileNumber = partner.MobileNumber,
                ProfileImage = partner.ProfileImage,
                StatusId = (int)partner.Status,
                Status = partner.Status.ToString(),
                CreatedAt = partner.CreatedOn.ToString("dd MMM yyyy"),
            });
    }

    public async Task<ApiResponse<FilterPagedResult<ServicePartnerListResponseDto>>> GetAllServicePartnersAsync(
        servicePartnerFilterDto filter)
    {
        var baseQuery = _context.ServicePartners
            .Where(x => !x.IsDeleted)
            .Select(x => new
            {
                Partner = x,
                JobsDone = _context.Bookings.IgnoreQueryFilters().Count(b =>
                    b.PartnerId == x.Id &&
                    b.BookingStatus == BookingStatus.Completed &&
                    b.PaymentStatus == PaymentStatus.Paid)
            });

        var jobRange = await baseQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Min = g.Min(x => (int?)x.JobsDone) ?? 0,
                Max = g.Max(x => (int?)x.JobsDone) ?? 0
            })
            .FirstOrDefaultAsync();

        var query = _context.ServicePartners
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && (!filter.StatusId.HasValue || x.Status == (PartnerStatus)filter.StatusId.Value)
                        && (!filter.ServiceTypeId.HasValue || x.ServiceTypeId == filter.ServiceTypeId.Value))
            .Select(x => new
            {
                Partner = x,
                JobsDone = _context.Bookings
                    .Count(b => b.PartnerId == x.Id
                                && b.BookingStatus == BookingStatus.Completed
                                && b.PaymentStatus == PaymentStatus.Paid)
            });

        if (filter.MinJob.HasValue)
            query = query.Where(x => x.JobsDone >= filter.MinJob.Value);

        if (filter.MaxJob.HasValue)
            query = query.Where(x => x.JobsDone <= filter.MaxJob.Value);

        if (!string.IsNullOrWhiteSpace(filter.PartnerName))
        {
            var normalisedName = filter.PartnerName
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            query = query.Where(x =>
                normalisedName.All(term =>
                    x.Partner.FullName.ToLower().Contains(term)));
        }

        bool isDesc = filter.SortOrder?.ToLower() != "asc";

        query = filter.SortBy?.ToLower() switch
        {
            "servicepartnerid" => isDesc
                ? query.OrderByDescending(x => x.Partner.Id)
                : query.OrderBy(x => x.Partner.Id),
            "fullname" => isDesc
                ? query.OrderByDescending(x => x.Partner.FullName)
                : query.OrderBy(x => x.Partner.FullName),
            "email" => isDesc
                ? query.OrderByDescending(x => x.Partner.Email)
                : query.OrderBy(x => x.Partner.Email),
            "jobsdone" => isDesc
                ? query.OrderByDescending(x => x.JobsDone)
                : query.OrderBy(x => x.JobsDone),
            _ => query.OrderByDescending(x => x.Partner.Id)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ServicePartnerListResponseDto
            {
                Id = x.Partner.Id,
                ServicePartnerId = $"{x.Partner.Id:D3}",
                FullName = x.Partner.FullName.ToTitleCase(),
                MobileNumber = x.Partner.MobileNumber,
                Email = x.Partner.Email,
                ResidentialAddress = x.Partner.ResidentialAddress,
                ServiceType = _context.ServiceTypes
                    .Where(st => st.Id == x.Partner.ServiceTypeId)
                    .Select(st => st.Name)
                    .FirstOrDefault() ?? string.Empty,
                JobsDone = x.JobsDone,
                StatusId = (int)x.Partner.Status,
                Status = x.Partner.Status.ToString(),
            })
            .ToListAsync();

        return ApiResponse<FilterPagedResult<ServicePartnerListResponseDto>>.SuccessResponse(
            "Service partners retrieved successfully",
            new FilterPagedResult<ServicePartnerListResponseDto>
            {
                Data = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Min = jobRange?.Min ?? 0,
                Max = jobRange?.Max ?? 0
            });
    }

    public async Task<ApiResponse<ServicePartnerDetailResponseDto>> GetServicePartnerByIdAsync(int id)
    {
        var partner = await _context.ServicePartners
            .Include(p => p.Experiences)
            .Include(p => p.Skills)
            .Include(p => p.ServicesOffered)
            .Include(p => p.Languages)
            .Include(p => p.Documents)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (partner == null)
            return ApiResponse<ServicePartnerDetailResponseDto>.Fail(
                $"Service partner with id {id} not found.");

        var serviceTypeName = await _context.ServiceTypes
            .AsNoTracking()
            .Where(st => st.Id == partner.ServiceTypeId)
            .Select(st => st.Name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var reviewStats = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.PartnerId == id && !r.IsDeleted)
            .GroupBy(r => r.PartnerId)
            .Select(g => new
            {
                AverageRating = Math.Round((decimal)g.Average(r => r.Rating), 1),
                TotalReviews = g.Count()
            })
            .FirstOrDefaultAsync();

        var dto = new ServicePartnerDetailResponseDto
        {
            Id = partner.Id,
            FullName = partner.FullName,
            Email = partner.Email,
            MobileNumber = partner.MobileNumber,
            ServiceType = serviceTypeName,
            ResidentialAddress = partner.ResidentialAddress,
            StatusId = (int)partner.Status,
            TotalExperienceYears = Math.Round(partner.Experiences.Sum(e =>
            {
                var end = e.ToDate ?? DateTime.UtcNow;
                return (end - e.FromDate).TotalDays / 365;
            }), 1),
            Experiences = partner.Experiences.Select(e => new PartnerExperiencesResponseDto
            {
                CompanyName = e.CompanyName,
                Role = e.Role,
                YearsOfExperience = Math.Round(((e.ToDate ?? DateTime.UtcNow) - e.FromDate).TotalDays / 365, 1)
            }).ToList(),
            Skills = await _context.Categories
                .Where(c => partner.Skills.Select(s => s.CategoryId).Contains(c.Id))
                .Select(c => c.Name)
                .ToListAsync(),
            ServicesOffered = await _context.SubCategories
                .Where(sc => partner.ServicesOffered.Select(s => s.SubCategoryId).Contains(sc.Id))
                .Select(sc => sc.Name)
                .ToListAsync(),
            Languages = partner.Languages.Select(l => new PartnerLanguagesResponseDto
            {
                Language = l.Language.ToString(),
            }).ToList(),
            Documents = partner.Documents.Select(d => new PartnerDocumentsResponseDto
            {
                DocumentName = d.DocumentName,
                FilePath = d.FilePath,
                FileSizeKb = d.FileSizeKb,
                FileType = d.FileType,
            }).ToList(),

            AverageRating = reviewStats?.AverageRating ?? 0,
            TotalReviews = reviewStats?.TotalReviews ?? 0,
        };

        return ApiResponse<ServicePartnerDetailResponseDto>.SuccessResponse(
            "Service partner fetched successfully.", dto);
    }

    public async Task<ApiResponse<bool>> ApproveServicePartnerAsync(int id, int adminUserId)
    {
        var partner = await _context.ServicePartners.FindAsync(id);

        if (partner is null || partner.IsDeleted)
            return ApiResponse<bool>.Fail("Service partner not found.");

        if (partner.Status == PartnerStatus.Active)
            return ApiResponse<bool>.Fail("Service partner is already approved.");

        partner.Status = PartnerStatus.Active;
        partner.ApprovedBy = adminUserId;
        partner.ApprovedOn = DateTime.UtcNow;
        partner.Activate();
        partner.SetModified(adminUserId);

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse("Service partner approved successfully.", true);
    }

    public async Task<ApiResponse<bool>> RejectServicePartnerAsync(int id, int adminUserId)
    {
        var partner = await _context.ServicePartners.FindAsync(id);

        if (partner is null || partner.IsDeleted)
            return ApiResponse<bool>.Fail("Service partner not found.");

        if (partner.Status == PartnerStatus.Rejected)
            return ApiResponse<bool>.Fail("Service partner is already rejected.");

        partner.Status = PartnerStatus.Rejected;
        partner.SetModified(adminUserId);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse("Service partner rejected successfully.");
    }

    public async Task<ApiResponse<List<DropdownOptionDto>>> GetServiceTypesAsync()
    {
        var items = await _context.ServiceTypes
            .Where(st => !st.IsDeleted)
            .OrderBy(st => st.Name)
            .Select(st => new DropdownOptionDto { Label = st.Name, Value = st.Id.ToString() })
            .AsNoTracking()
            .ToListAsync();

        return ApiResponse<List<DropdownOptionDto>>.SuccessResponse("Service types fetched.", items);
    }

    public async Task<ApiResponse<List<DropdownOptionDto>>> GetCategoriesByServiceTypeAsync(int serviceTypeId)
    {
        var items = await _context.Categories
            .Where(c => c.ServiceTypeId == serviceTypeId && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new DropdownOptionDto { Label = c.Name, Value = c.Id.ToString() })
            .AsNoTracking()
            .ToListAsync();

        return ApiResponse<List<DropdownOptionDto>>.SuccessResponse("Categories fetched.", items);
    }

    public async Task<ApiResponse<List<DropdownOptionDto>>> GetSubCategoriesByCategoryAsync(int categoryId)
    {
        var items = await _context.SubCategories
            .Where(sc => sc.CategoryId == categoryId && !sc.IsDeleted)
            .OrderBy(sc => sc.Name)
            .Select(sc => new DropdownOptionDto { Label = sc.Name, Value = sc.Id.ToString() })
            .AsNoTracking()
            .ToListAsync();

        return ApiResponse<List<DropdownOptionDto>>.SuccessResponse("Sub-categories fetched.", items);
    }

    public async Task<ApiResponse<bool>> UpdateStatusAsync(int id)
    {
        var partner = await _context.ServicePartners.FindAsync(id);

        if (partner is null)
            return ApiResponse<bool>.Fail("Service partner not found.");

        if (partner.Status == PartnerStatus.Active)
        {
            partner.Status = PartnerStatus.Inactive;
            partner.Deactivate();
        }
        else if (partner.Status == PartnerStatus.Inactive || partner.Status == PartnerStatus.Pending)
        {
            partner.Status = PartnerStatus.Active;
            partner.Activate();
        }
        else
        {
            return ApiResponse<bool>.Fail("Only Active or Inactive partners can be toggled.");
        }

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(
            $"Service partner {(partner.Status == PartnerStatus.Active ? "activated" : "deactivated")} successfully.",
            true);
    }

    public async Task<ApiResponse<bool>> DeleteServicePartnerAsync(int id)
    {
        var partner = await _context.ServicePartners.FindAsync(id);

        if (partner is null)
            return ApiResponse<bool>.Fail("Service partner not found.");

        partner.SoftDelete(true);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse("Service partner deleted successfully.", true);
    }

    public async Task<ApiResponse<PagedResult<PartnerAssignedServiceDto>>> GetAssignedServicesAsync(
        int partnerId,
        PartnerAssignedServiceFilterDto filter)
    {
        var query =
            from b in _context.Bookings
            join s in _context.Services on b.ServiceId equals s.Id
            join sc in _context.SubCategories on s.SubCategoryId equals sc.Id
            join c in _context.Customers on b.CustomerId equals c.Id
            join a in _context.Addresses on b.AddressId equals a.Id
            where b.PartnerId == partnerId && !b.IsDeleted && b.BookingStatus != BookingStatus.Failed
            select new { Booking = b, Service = s, SubCategory = sc, Customer = c, Address = a };

        if (!string.IsNullOrWhiteSpace(filter.Date)
            && DateOnly.TryParse(filter.Date, out var filterDate))
            query = query.Where(x => x.Booking.SlotDate == filterDate);

        if (!string.IsNullOrWhiteSpace(filter.Time)
            && TimeOnly.TryParse(filter.Time, out var filterTime))
            query = query.Where(x =>
                x.Booking.SlotStartTime.Hour == filterTime.Hour &&
                x.Booking.SlotStartTime.Minute == filterTime.Minute);

        if (!string.IsNullOrWhiteSpace(filter.Status)
            && Enum.TryParse<BookingStatus>(filter.Status, ignoreCase: true, out var statusEnum))
            query = query.Where(x => x.Booking.BookingStatus == statusEnum);

        bool isDesc = filter.SortOrder?.ToLower() != "asc";

        query = filter.SortBy?.ToLower() switch
        {
            "servicename" => isDesc
                ? query.OrderByDescending(x => x.Service.Name)
                : query.OrderBy(x => x.Service.Name),
            "customername" => isDesc
                ? query.OrderByDescending(x => x.Customer.Name)
                : query.OrderBy(x => x.Customer.Name),
            "datetime" => isDesc
                ? query.OrderByDescending(x => x.Booking.SlotDate)
                       .ThenByDescending(x => x.Booking.SlotStartTime)
                : query.OrderBy(x => x.Booking.SlotDate)
                       .ThenBy(x => x.Booking.SlotStartTime),
            "status" => isDesc
                ? query.OrderByDescending(x => x.Booking.BookingStatus)
                : query.OrderBy(x => x.Booking.BookingStatus),
            _ => query.OrderByDescending(x => x.Booking.Id)
        };

        var totalCount = await query.AsNoTracking().CountAsync();

        var items = await query
            .AsNoTracking()
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new PartnerAssignedServiceDto
            {
                CustomerId = x.Booking.CustomerId,
                ServiceName = x.Service.Name,
                CustomerName = x.Customer.Name,
                DateTime = x.Booking.SlotDate.ToString("dd MMM yyyy")
                           + " " + x.Booking.SlotStartTime.ToString("hh:mm tt"),
                ServiceAddress = x.Address.HouseFlatNo + ", "
                               + x.Address.Landmark + ", "
                               + x.Address.DisplayName,
                StatusId = (int)x.Booking.BookingStatus,
                Status = x.Booking.BookingStatus.ToString(),
            })
            .ToListAsync();

        return ApiResponse<PagedResult<PartnerAssignedServiceDto>>.SuccessResponse(
            "Assigned services fetched successfully.",
            new PagedResult<PartnerAssignedServiceDto>
            {
                Data = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            });
    }

    public async Task<ApiResponse<PartnerProfileResponseDto>> GetPartnerProfileAsync(int partnerId)
    {
        var partner = await _context.ServicePartners
            .Include(p => p.Educations)
            .Include(p => p.Experiences)
            .Include(p => p.Skills)
            .Include(p => p.ServicesOffered)
            .Include(p => p.Languages)
            .Include(p => p.Documents)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == partnerId && !p.IsDeleted);

        if (partner is null)
            return ApiResponse<PartnerProfileResponseDto>.Fail("Partner not found.");

        var serviceTypeName = await _context.ServiceTypes
            .AsNoTracking()
            .Where(st => st.Id == partner.ServiceTypeId)
            .Select(st => st.Name)
            .FirstOrDefaultAsync() ?? string.Empty;

        var skillNames = await _context.Categories
            .Where(c => partner.Skills.Select(s => s.CategoryId).Contains(c.Id))
            .Select(c => c.Name)
            .ToListAsync();

        var serviceNames = await _context.SubCategories
            .Where(sc => partner.ServicesOffered.Select(s => s.SubCategoryId).Contains(sc.Id))
            .Select(sc => sc.Name)
            .ToListAsync();

        var dto = new PartnerProfileResponseDto
        {
            Id = partner.Id,
            FullName = partner.FullName,
            Email = partner.Email,
            MobileNumber = partner.MobileNumber,
            DateOfBirth = partner.DateOfBirth.ToString("yyyy-MM-dd"),
            Gender = partner.Gender.ToString(),
            PermanentAddress = partner.PermanentAddress,
            ResidentialAddress = partner.ResidentialAddress,
            ProfileImage = partner.ProfileImage,
            ServiceType = serviceTypeName,
            ServiceTypeId = partner.ServiceTypeId,
            StatusId = (int)partner.Status,
            Status = partner.Status.ToString(),
            Educations = partner.Educations.Select(e => new EducationInfoDto
            {
                SchoolCollege = e.InstituteName,
                PassingYear = e.PassingYear.ToString(),
                Marks = e.MarksPercentage,
            }).ToList(),
            ProfessionalExperiences = partner.Experiences.Select(e => new ProfessionalInfoDto
            {
                CompanyName = e.CompanyName,
                Role = e.Role,
                FromDate = e.FromDate.ToString("yyyy-MM-dd"),
                ToDate = e.ToDate?.ToString("yyyy-MM-dd"),
            }).ToList(),
            Languages = partner.Languages.Select(l => new LanguageDto
            {
                Language = l.Language.ToString(),
                Proficiency = l.Proficiency.ToString(),
            }).ToList(),
            Documents = partner.Documents.Select(d => new PartnerDocumentDto
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                FilePath = d.FilePath,
                FileSizeKb = d.FileSizeKb,
                FileType = d.FileType,
            }).ToList(),
            Skills = skillNames,
            ServicesOffered = serviceNames,
        };

        return ApiResponse<PartnerProfileResponseDto>.SuccessResponse("Profile fetched.", dto);
    }

    public async Task<ApiResponse<PartnerProfileResponseDto>> UpdatePartnerProfileAsync(
        int partnerId,
        UpdateServicePartnerProfileRequestDto dto)
    {
        var partner = await _context.ServicePartners
            .Include(p => p.Educations)
            .Include(p => p.Experiences)
            .Include(p => p.Languages)
            .Include(p => p.Documents)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == partnerId && !p.IsDeleted);

        if (partner is null)
            return ApiResponse<PartnerProfileResponseDto>.Fail("Partner not found.");

        var personal = Deserialize<PersonalDetailDto>(dto.PersonalDetail) ?? new();
        var educations = Deserialize<List<EducationInfoDto>>(dto.EducationInfoList) ?? new();
        var professionals = Deserialize<List<ProfessionalInfoDto>>(dto.ProfessionalInfoList) ?? new();
        var languages = Deserialize<List<LanguageDto>>(dto.LanguageList) ?? new();
        var removedIds = Deserialize<List<int>>(dto.RemovedDocumentIds) ?? new();

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(personal.FullName))
            errors.Add("Full name is required.");

        if (!DateTime.TryParse(personal.DateOfBirth, out var dateOfBirth))
            errors.Add("Valid date of birth is required.");

        if (!string.IsNullOrWhiteSpace(personal.Email) && !EmailRegex.IsMatch(personal.Email))
            errors.Add("Email format is invalid.");

        var validEducations = educations
            .Where(e => !string.IsNullOrWhiteSpace(e.SchoolCollege)
                     || !string.IsNullOrWhiteSpace(e.PassingYear)
                     || e.Marks != null)
            .ToList();

        if (!validEducations.Any())
            errors.Add("At least one educational qualification is required.");

        var currentYear = (short)DateTime.UtcNow.Year;
        foreach (var edu in validEducations)
        {
            if (string.IsNullOrWhiteSpace(edu.SchoolCollege))
                errors.Add("School/College name is required for all education entries.");

            if (string.IsNullOrWhiteSpace(edu.PassingYear))
                errors.Add($"Passing year is required for '{edu.SchoolCollege}'.");
            else if (!short.TryParse(edu.PassingYear, out var year))
                errors.Add($"Invalid passing year: '{edu.PassingYear}'.");
            else if (year >= currentYear)
                errors.Add($"Passing year '{year}' must be less than current year ({currentYear}).");

            if (edu.Marks == null)
                errors.Add($"Marks are required for '{edu.SchoolCollege}'.");
        }

        var validProfessionals = professionals
            .Where(e => !string.IsNullOrWhiteSpace(e.CompanyName)
                     || !string.IsNullOrWhiteSpace(e.Role)
                     || !string.IsNullOrWhiteSpace(e.FromDate)
                     || !string.IsNullOrWhiteSpace(e.ToDate))
            .ToList();

        foreach (var exp in validProfessionals)
        {
            if (string.IsNullOrWhiteSpace(exp.CompanyName))
                errors.Add("Company name is required for all professional entries.");
            if (string.IsNullOrWhiteSpace(exp.Role))
                errors.Add("Role is required for all professional entries.");
            if (!string.IsNullOrWhiteSpace(exp.FromDate) && !string.IsNullOrWhiteSpace(exp.ToDate))
            {
                if (DateTime.TryParse(exp.FromDate, out var fd) && DateTime.TryParse(exp.ToDate, out var td))
                    if (td <= fd)
                        errors.Add($"To date must be after from date for '{exp.CompanyName}'.");
            }
        }

        if (!languages.Any())
            errors.Add("At least one language is required.");

        var remainingExistingCount = partner.Documents.Count - removedIds.Count;
        var newFilesCount = dto.AttachmentFiles.Count;
        if (remainingExistingCount + newFilesCount < 1)
            errors.Add("At least one document is required.");

        foreach (var file in dto.AttachmentFiles)
        {
            if (!AllowedMimeTypes.Contains(file.ContentType))
                errors.Add($"'{file.FileName}': only PDF, JPG, and PNG are allowed.");
            if (file.Length > 20 * 1024 * 1024)
                errors.Add($"'{file.FileName}': file size must not exceed 20 MB.");
        }

        var duplicateSchools = validEducations
            .GroupBy(e => e.SchoolCollege?.Trim().ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateSchools.Any())
            errors.Add($"Duplicate school entries: {string.Join(", ", duplicateSchools)}.");

        var duplicateCompanies = validProfessionals
            .GroupBy(e => e.CompanyName?.Trim().ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateCompanies.Any())
            errors.Add($"Duplicate company entries: {string.Join(", ", duplicateCompanies)}.");

        var duplicateLanguages = languages
            .GroupBy(l => l.Language?.Trim().ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateLanguages.Any())
            errors.Add($"Duplicate language entries: {string.Join(", ", duplicateLanguages)}.");

        if (errors.Any())
            return ApiResponse<PartnerProfileResponseDto>.Fail(errors.First());

        if (!string.IsNullOrWhiteSpace(personal.FullName))
            partner.FullName = personal.FullName.Trim().ToTitleCase();
        if (dateOfBirth != default)
            partner.DateOfBirth = DateTime.SpecifyKind(dateOfBirth, DateTimeKind.Utc);

        if (!string.IsNullOrWhiteSpace(personal.MobileNumber))
        {
            var normalizedMobile = personal.MobileNumber.Trim();
            var mobileExists = await _context.ServicePartners
                .AnyAsync(p => p.MobileNumber == normalizedMobile && p.Id != partnerId);
            if (mobileExists)
                return ApiResponse<PartnerProfileResponseDto>.Fail("Mobile number already exists. Please use another number.");
            partner.MobileNumber = normalizedMobile;
        }

        if (!string.IsNullOrWhiteSpace(personal.Email))
        {
            var normalizedEmail = personal.Email.Trim().ToLower();
            var emailExists = await _context.ServicePartners
                .AnyAsync(p => p.Email == normalizedEmail && p.Id != partnerId);
            if (emailExists)
                return ApiResponse<PartnerProfileResponseDto>.Fail("Email already exists. Please use another email.");
            partner.Email = normalizedEmail;
        }

        if (!string.IsNullOrWhiteSpace(personal.PermanentAddress))
            partner.PermanentAddress = personal.PermanentAddress.Trim();
        if (!string.IsNullOrWhiteSpace(personal.ResidentialAddress))
            partner.ResidentialAddress = personal.ResidentialAddress.Trim();

        Enum.TryParse(personal.Gender?.Trim(), ignoreCase: true, out Gender gender);
        partner.Gender = gender;

        if (dto.ProfileImage is { Length: > 0 })
        {
            // ✅ Delete old profile image from Cloudinary
            await DeleteFileAsync(partner.ProfileImage);
            partner.ProfileImage = await SaveFileAsync(dto.ProfileImage, "partner-profiles");
        }

        _context.PartnerEducations.RemoveRange(partner.Educations);
        partner.Educations.Clear();
        foreach (var edu in validEducations)
        {
            short.TryParse(edu.PassingYear, out var yr);
            var entry = new PartnerEducationEntity
            {
                InstituteName = edu.SchoolCollege!.Trim(),
                PassingYear = yr,
                MarksPercentage = edu.Marks,
            };
            entry.SetCreated(0);
            partner.Educations.Add(entry);
        }

        _context.PartnerExperiences.RemoveRange(partner.Experiences);
        partner.Experiences.Clear();
        foreach (var exp in validProfessionals.Where(e =>
            !string.IsNullOrWhiteSpace(e.CompanyName) && !string.IsNullOrWhiteSpace(e.Role)))
        {
            DateTime.TryParse(exp.FromDate, out var fd);
            DateTime? td = DateTime.TryParse(exp.ToDate, out var t) ? t : null;
            var entry = new PartnerExperienceEntity
            {
                CompanyName = exp.CompanyName!.Trim(),
                Role = exp.Role!.Trim(),
                FromDate = DateTime.SpecifyKind(fd, DateTimeKind.Utc),
                ToDate = td.HasValue ? DateTime.SpecifyKind(td.Value, DateTimeKind.Utc) : null,
            };
            entry.SetCreated(0);
            partner.Experiences.Add(entry);
        }

        _context.PartnerLanguages.RemoveRange(partner.Languages);
        partner.Languages.Clear();
        foreach (var l in languages)
        {
            Enum.TryParse(l.Language?.Trim(), ignoreCase: true, out Language langEnum);
            Enum.TryParse(l.Proficiency?.Trim(), ignoreCase: true, out Proficiency profEnum);
            var entry = new PartnerLanguageEntity { Language = langEnum, Proficiency = profEnum };
            entry.SetCreated(0);
            partner.Languages.Add(entry);
        }

        if (removedIds.Any())
        {
            var docsToRemove = partner.Documents.Where(d => removedIds.Contains(d.Id)).ToList();
            foreach (var doc in docsToRemove)
            {
                // ✅ Delete from Cloudinary instead of local disk
                await DeleteFileAsync(doc.FilePath);
                _context.PartnerDocuments.Remove(doc);
            }
        }

        partner.SetModified(_currentUser.UserId);
        await _context.SaveChangesAsync();

        foreach (var file in dto.AttachmentFiles)
        {
            var filePath = await SaveFileAsync(file, "partner-documents");
            var docEntry = new PartnerDocumentEntity
            {
                PartnerId = partner.Id,
                DocumentName = Path.GetFileNameWithoutExtension(file.FileName),
                FilePath = filePath,
                FileSizeKb = (int)(file.Length / 1024),
                FileType = Path.GetExtension(file.FileName).TrimStart('.').ToLower(),
            };
            docEntry.SetCreated(0);
            _context.PartnerDocuments.Add(docEntry);
        }

        await _context.SaveChangesAsync();

        return await GetPartnerProfileAsync(partnerId);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var isImage = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        return isImage
            ? await _cloudinary.UploadImageAsync(file, folder)
            : await _cloudinary.UploadFileAsync(file, folder);
    }

    private async Task DeleteFileAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        var publicId = _cloudinary.ExtractPublicId(url);
        if (publicId != null)
            await _cloudinary.DeleteAsync(publicId);
    }

    private static T? Deserialize<T>(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, JsonOpts);
}