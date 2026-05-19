
using Homecare.Application.Constants;
using Homecare.Application.Constants.Pagination;
using Homecare.Application.DTOs.PartnerLeave;
using Homecare.Application.Interfaces.Notification;
using Homecare.Application.Interfaces.PartnerLeave;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Leave;

public class LeaveService : ILeaveService
{
    private readonly AppDbContext _context;
    private readonly IPartnerSystemNotifService _partnerNotif;
    private readonly IAdminSystemNotifService _adminNotif;

    public LeaveService(
        AppDbContext context,
        IPartnerSystemNotifService partnerNotif,
        IAdminSystemNotifService adminNotif)
    {
        _context = context;
        _partnerNotif = partnerNotif;
        _adminNotif = adminNotif;
    }

    public async Task<ApiResponse<LeaveResponseDto>> ApplyLeaveAsync(
        int partnerId, ApplyLeaveRequestDto dto)
    {
        if (!DateOnly.TryParse(dto.FromDate, out var fromDate))
            return ApiResponse<LeaveResponseDto>.Fail("Invalid from date format. Use yyyy-MM-dd.");

        if (!DateOnly.TryParse(dto.ToDate, out var toDate))
            return ApiResponse<LeaveResponseDto>.Fail("Invalid to date format. Use yyyy-MM-dd.");

        if (fromDate < DateOnly.FromDateTime(DateTime.Today))
            return ApiResponse<LeaveResponseDto>.Fail("Leave start date cannot be in the past.");

        if (toDate < fromDate)
            return ApiResponse<LeaveResponseDto>.Fail("To date must be on or after from date.");

        if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Trim().Length < 10)
            return ApiResponse<LeaveResponseDto>.Fail("Reason must be at least 10 characters.");

        var overlap = await _context.PartnerLeaves.AnyAsync(l =>
            l.PartnerId == partnerId &&
            (l.Status == LeaveStatus.Pending || l.Status == LeaveStatus.Approved) &&
            l.FromDate <= toDate &&
            l.ToDate >= fromDate);

        if (overlap)
            return ApiResponse<LeaveResponseDto>.Fail(
                "You already have a pending or approved leave overlapping these dates.");

        var partner = await _context.ServicePartners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == partnerId);

        if (partner is null)
            return ApiResponse<LeaveResponseDto>.Fail("Partner not found.");

        var leave = new PartnerLeave
        {
            PartnerId = partnerId,
            FromDate = fromDate,
            ToDate = toDate,
            Reason = dto.Reason.Trim(),
            Status = LeaveStatus.Pending
        };
        leave.SetCreated(partnerId);

        _context.PartnerLeaves.Add(leave);
        await _context.SaveChangesAsync();


        await _adminNotif.SendToAllAdminsAsync(
            title: "New Leave Request",
            message: $"{partner.FullName} applied for leave from " +
                     $"{fromDate:dd MMM yyyy} to {toDate:dd MMM yyyy}.",
            type: AdminSystemNotifType.NewLeaveRequest,
            referenceId: leave.Id,
            referenceType: "Leave",
            fromPartnerId: partnerId,
            fromPartnerName: partner.FullName
        );

        return ApiResponse<LeaveResponseDto>.SuccessResponse(
            "Leave application submitted successfully.",
            MapToDto(leave));
    }

    public async Task<ApiResponse<PagedResult<LeaveResponseDto>>> GetMyLeavesAsync(
        int partnerId, LeaveFilterDto filter)
    {
        var query = _context.PartnerLeaves
            .Where(l => l.PartnerId == partnerId)
            .AsQueryable();

        if (filter.StatusId.HasValue)
            query = query.Where(l => l.Status == (LeaveStatus)filter.StatusId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(l => new LeaveResponseDto
            {
                Id = l.Id,
                FromDate = l.FromDate.ToString("dd MMM yyyy"),
                ToDate = l.ToDate.ToString("dd MMM yyyy"),
                TotalDays = l.ToDate.DayNumber - l.FromDate.DayNumber + 1,
                Reason = l.Reason,
                StatusId = (int)l.Status,
                Status = l.Status.ToString(),
                AdminRemarks = l.AdminRemarks,
                AppliedOn = l.CreatedOn.ToString("dd MMM yyyy"),
                ReviewedAt = l.ReviewedAt.HasValue
                    ? l.ReviewedAt.Value.ToString("dd MMM yyyy") : null
            })
            .ToListAsync();

        return ApiResponse<PagedResult<LeaveResponseDto>>.SuccessResponse(
            "Leaves fetched successfully.",
            new PagedResult<LeaveResponseDto>
            {
                Data = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            });
    }

    public async Task<ApiResponse<string>> CancelLeaveAsync(int leaveId, int partnerId)
    {
        var leave = await _context.PartnerLeaves
            .FirstOrDefaultAsync(l => l.Id == leaveId && l.PartnerId == partnerId);

        if (leave is null)
            return ApiResponse<string>.Fail("Leave request not found.");

        if (leave.Status != LeaveStatus.Pending)
            return ApiResponse<string>.Fail("Only pending leave requests can be cancelled.");

        leave.Status = LeaveStatus.Cancelled;
        leave.SetModified(partnerId);
        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(
            "Leave request cancelled successfully.", "Cancelled");
    }

    public async Task<ApiResponse<PagedResult<AdminLeaveListDto>>> GetAllLeavesAsync(
        LeaveFilterDto filter)
    {
        var query = _context.PartnerLeaves
            .Include(l => l.Partner)
            .AsQueryable();

        if (filter.StatusId.HasValue)
            query = query.Where(l => l.Status == (LeaveStatus)filter.StatusId.Value);

        if (!string.IsNullOrWhiteSpace(filter.PartnerName))
        {
            var name = filter.PartnerName.Trim().ToLower();
            query = query.Where(l => l.Partner.FullName.ToLower().Contains(name));
        }

        if (DateOnly.TryParse(filter.FromDate, out var fd))
            query = query.Where(l => l.FromDate >= fd);

        if (DateOnly.TryParse(filter.ToDate, out var td))
            query = query.Where(l => l.ToDate <= td);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(l => new AdminLeaveListDto
            {
                Id = l.Id,
                PartnerId = l.PartnerId,
                PartnerName = l.Partner.FullName,
                PartnerEmail = l.Partner.Email,
                ProfileImage = l.Partner.ProfileImage,
                FromDate = l.FromDate.ToString("dd MMM yyyy"),
                ToDate = l.ToDate.ToString("dd MMM yyyy"),
                TotalDays = l.ToDate.DayNumber - l.FromDate.DayNumber + 1,
                Reason = l.Reason,
                StatusId = (int)l.Status,
                Status = l.Status.ToString(),
                AdminRemarks = l.AdminRemarks,
                AppliedOn = l.CreatedOn.ToString("dd MMM yyyy"),
                ReviewedAt = l.ReviewedAt.HasValue
                    ? l.ReviewedAt.Value.ToString("dd MMM yyyy") : null
            })
            .ToListAsync();

        return ApiResponse<PagedResult<AdminLeaveListDto>>.SuccessResponse(
            "Leave requests fetched successfully.",
            new PagedResult<AdminLeaveListDto>
            {
                Data = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            });
    }

    public async Task<ApiResponse<string>> ReviewLeaveAsync(
        int leaveId, int adminId, ReviewLeaveDto dto)
    {
        var leave = await _context.PartnerLeaves
            .Include(l => l.Partner)
            .FirstOrDefaultAsync(l => l.Id == leaveId);

        if (leave is null)
            return ApiResponse<string>.Fail("Leave request not found.");

        if (leave.Status != LeaveStatus.Pending)
            return ApiResponse<string>.Fail("Only pending leave requests can be reviewed.");

        leave.Status = dto.IsApproved ? LeaveStatus.Approved : LeaveStatus.Rejected;
        leave.ReviewedBy = adminId;
        leave.ReviewedAt = DateTime.UtcNow;
        leave.AdminRemarks = dto.AdminRemarks?.Trim();
        leave.SetModified(adminId);


        var today = DateOnly.FromDateTime(DateTime.Today);
        if (dto.IsApproved && leave.FromDate <= today && leave.ToDate >= today)
            leave.Partner.Status = PartnerStatus.Onleave;

        await _context.SaveChangesAsync();


        var action = dto.IsApproved ? "approved" : "rejected";
        var remarksText = string.IsNullOrWhiteSpace(dto.AdminRemarks)
            ? string.Empty
            : $" Admin remarks: {dto.AdminRemarks}";

        await _partnerNotif.SendAsync(
            partnerId: leave.PartnerId,
            title: dto.IsApproved ? "Leave Approved" : "Leave Rejected",
            message: $"Your leave request ({leave.FromDate:dd MMM yyyy} – " +
                     $"{leave.ToDate:dd MMM yyyy}) has been {action}.{remarksText}",
            type: dto.IsApproved
                ? PartnerSystemNotifType.LeaveApproved
                : PartnerSystemNotifType.LeaveRejected,
            referenceId: leave.Id,
            referenceType: "Leave"
        );

        return ApiResponse<string>.SuccessResponse(
            $"Leave {action} successfully.",
            leave.Status.ToString());
    }

    private static LeaveResponseDto MapToDto(PartnerLeave l) => new()
    {
        Id = l.Id,
        FromDate = l.FromDate.ToString("dd MMM yyyy"),
        ToDate = l.ToDate.ToString("dd MMM yyyy"),
        TotalDays = l.ToDate.DayNumber - l.FromDate.DayNumber + 1,
        Reason = l.Reason,
        StatusId = (int)l.Status,
        Status = l.Status.ToString(),
        AppliedOn = l.CreatedOn.ToString("dd MMM yyyy")
    };
}