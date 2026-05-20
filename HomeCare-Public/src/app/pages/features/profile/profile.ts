import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OtpStateService } from '../../../core/services/auth/otp-state-service';
import { MatDialog } from '@angular/material/dialog';
import { FormModal } from '../../../shared/components/form-modal/form-modal';
import { IFormModalData } from '../../../core/models/shared/IFormModalData.model';
import { Toaster } from '../../../core/services/toaster/toaster';
import { MapPicker } from './map-picker/map-picker';
import { LocationSearchModal } from './location-search-modal/location-search-modal';
import { of, tap } from 'rxjs';
import { IActionItem } from '../../../core/models/shared/IActionDropDownModel';
import { ActionDropdown } from '../../../shared/components/action-dropdown/action-dropdown';
import { ISavedAddress } from '../../../core/models/profile/ISavedAddress';
import { IAddressRequest } from '../../../core/models/profile/IAddressRequest';
import { ProfileService } from '../../../core/services/profile/profile-service';
import { DeleteConfirmation } from '../../../shared/components/delete-confirmation/delete-confirmation';
import { PROFILE_MESSAGES } from '../../../core/constants/profile-messages';
import { IReferralInfo, IWallet } from '../../../core/models/referral/IReferral';
import { ReferralService } from '../../../core/services/referral/referral-service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, MapPicker, LocationSearchModal, ActionDropdown],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  mobileNumber: string | null = null;
  email: string = '';
  editMode: boolean = false;
  editData: ISavedAddress | null = null;

  showSearchModal = false;
  showMapModal = false;
  referralInfo: IReferralInfo | null = null;
  wallet: IWallet | null = null;
  referralCopied = false;

  ngOnInit(): void {
    this.loadProfile();
    this.loadReferral();
    this.loadWallet();
  }

  pendingLat: number = 20.5937;
  pendingLng: number = 78.9629;
  pendingAddress: string = '';

  actions: IActionItem[] = [
    { label: 'Edit', icon: 'bi-pencil', action: 'edit' },
    { label: 'Delete', icon: 'bi-trash', action: 'delete' },
  ];

  addresses: ISavedAddress[] = [];
  isLoading = false;

  constructor(
    private profileService: ProfileService,
    private otpStateService: OtpStateService,
    private dialog: MatDialog,
    private toaster: Toaster,
    private referralSvc: ReferralService
  ) {}

  loadProfile() {
    this.isLoading = true;
    this.profileService.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.email = res.data.email.toLowerCase();
          this.mobileNumber = res.data.mobileNumber ?? '';
          this.addresses = res.data.addresses;
        } else {
          this.toaster.error(res.message);
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.toaster.error(err.message);
      },
    });
  }
  loadReferral(): void {
    this.referralSvc.getReferralInfo().subscribe({
      next: (res) => {
        if (res.success) this.referralInfo = res.data;
      },
    });
  }

  loadWallet(): void {
    this.referralSvc.getWallet().subscribe({
      next: (res) => {
        if (res.success) this.wallet = res.data;
      },
    });
  }

  copyReferralCode(): void {
    if (!this.referralInfo?.referralCode) return;
    navigator.clipboard.writeText(this.referralInfo.referralCode).then(() => {
      this.referralCopied = true;
      setTimeout(() => (this.referralCopied = false), 2000);
    });
  }

  shareReferralCode(): void {
    if (!this.referralInfo?.referralCode) return;
    const url = `${window.location.origin}/login?ref=${this.referralInfo.referralCode}`;
    if (navigator.share) {
      navigator.share({ title: 'HomeCare', text: 'Use my code to get $2 off!', url });
    } else {
      navigator.clipboard.writeText(url);
      this.toaster.success('Referral link copied!');
    }
  }
  getRefereeStatusClass(status: string): string {
    switch (status) {
      case 'Rewarded':
        return 'badge-rewarded';
      case 'Cancelled':
        return 'badge-cancelled';
      default:
        return 'badge-pending-ref';
    }
  }

  onChangeEmailClick() {
    if (this.otpStateService.isEmailChangeOtpActive()) {
      const pendingEmail = this.otpStateService.getPendingEmail()!;
      this.openVerifyEmailOtpModal(pendingEmail?.toLowerCase() || 'your new email');
    } else {
      this.openChangeEmailModal();
    }
  }

  openPhoneModal() {
    const modalData: IFormModalData = {
      title: this.mobileNumber ? 'Change Mobile Number' : 'Add Mobile Number',
      fields: [{ name: 'newMobileNumber', label: 'Mobile Number', type: 'tel', required: true }],
      submitLabel: 'Save',
      initialData: { newMobileNumber: this.mobileNumber },
      onSubmit: ({ newMobileNumber }) => {
        if (newMobileNumber == this.mobileNumber) {
          this.toaster.error(PROFILE_MESSAGES.MOBILE_NUMBER.NEW_MOBILE_VALIDATION);
          dialogRef.close();
          return of(null);
        }

        return this.profileService.updateMobile(newMobileNumber).pipe(
          tap((res) => {
            if (res.success) {
              this.mobileNumber = newMobileNumber;
              this.toaster.success(res.message);
              dialogRef.close();
            } else {
              this.toaster.error(res.message);
            }
          })
        );
      },
    };
    const dialogRef = this.dialog.open(FormModal, { data: modalData, width: '450px' });
  }

  openChangeEmailModal() {
    const modalData: IFormModalData = {
      title: 'Change Email',
      fields: [{ name: 'newEmail', label: 'New Email', type: 'email', required: true }],
      submitLabel: 'Get OTP',
      initialData: { newEmail: this.email },
      onSubmit: ({ newEmail }) => {
        const normalizedEmail = newEmail?.toLowerCase();

        if (normalizedEmail === this.email) {
          this.toaster.error(PROFILE_MESSAGES.EMAIL.NEW_EMAIL_VALIDATION);
          dialogRef.close();
          return of(null);
        }

        return this.profileService.requestEmailChange({ newEmail: normalizedEmail }).pipe(
          tap((res) => {
            if (res.success) {
              this.toaster.success(res.message);
              this.otpStateService.setEmailChangeOtpRequested(normalizedEmail);
              this.otpStateService.setEmailChangeExpiresAt(
                res.data?.expiresAt ?? res.data?.ExpiresAt
              );
              dialogRef.close();
              this.openVerifyEmailOtpModal(normalizedEmail);
            } else {
              this.toaster.error(res.message);
            }
          })
        );
      },
    };
    const dialogRef = this.dialog.open(FormModal, { data: modalData, width: '450px' });
  }

  openVerifyEmailOtpModal(pendingEmail: string) {
    const modalData: IFormModalData = {
      title: 'Verify OTP',
      fields: [{ name: 'otp', label: 'OTP', type: 'otp', required: true }],
      submitLabel: 'Verify',
      subtitle: `We have sent a code on ${pendingEmail}`,
      onSubmit: ({ otp }) =>
        this.profileService.verifyEmailChange({ newEmail: pendingEmail, otp }).pipe(
          tap((res) => {
            if (res.success) {
              this.email = pendingEmail;
              this.otpStateService.clearEmailChangeOtp();
              this.toaster.success(res.message);
              dialogRef.close();
            } else {
              this.toaster.error(res.message);
            }
          })
        ),
    };
    const dialogRef = this.dialog.open(FormModal, { data: modalData, width: '450px' });
  }

  onAction(event: string, index: number) {
    const address = this.addresses[index];
    if (event === 'delete') {
      this.deleteAddress(address);
    } else if (event === 'edit') {
      this.editMode = true;
      this.editData = address;
      this.pendingLat = address.latitude ?? 20.5937;
      this.pendingLng = address.longitude ?? 78.9629;
      this.pendingAddress = address.displayName ?? '';
      this.showMapModal = true;
      this.showSearchModal = false;
      document.body.style.overflow = 'hidden';
    }
  }

  deleteAddress(address: ISavedAddress) {
    const dialogRef = this.dialog.open(DeleteConfirmation, {
      width: '400px',
      disableClose: true,
      data: {
        message:
          PROFILE_MESSAGES.ADDRESS.DELETE_ADDRESS_CONFIRMATION +
          ` <strong>${address.houseFlatNo},${address.landmark}</strong>?`,
        apiCall: () => this.profileService.deleteAddress(address.id!),
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadProfile();
      }
    });
  }

  openSearchModal() {
    this.showSearchModal = true;
    this.showMapModal = false;
    document.body.style.overflow = 'hidden';
  }

  onLocationSelectedFromSearch(event: { lat: number; lng: number; display_name: string }) {
    this.pendingLat = event.lat;
    this.pendingLng = event.lng;
    this.pendingAddress = event.display_name;
    this.showSearchModal = false;
    this.showMapModal = true;
  }

  onChangeLocation() {
    this.showMapModal = false;
    this.showSearchModal = true;
  }

  onAddressSelected(address: ISavedAddress) {
    if (this.editMode && address.id) {
      const dto: IAddressRequest = {
        houseFlatNo: address.houseFlatNo,
        landmark: address.landmark,
        label: address.label,
        latitude: address.latitude,
        longitude: address.longitude,
        displayName: address.displayName,
      };
      this.profileService.editAddress(address.id, dto).subscribe({
        next: (res) => {
          if (res.success) {
            this.toaster.success(res.message);
            this.loadProfile();
            this.closeAllModals();
          } else {
            this.toaster.error(res.message);
            this.showMapModal = false;
            this.editMode = false;
            this.editData = null;
          }
        },
        error: () => this.toaster.error(PROFILE_MESSAGES.ADDRESS.UPDATE_ADDRESS_FAIL),
      });
    } else {
      const dto: IAddressRequest = {
        houseFlatNo: address.houseFlatNo,
        landmark: address.landmark,
        label: address.label,
        latitude: address.latitude,
        longitude: address.longitude,
        displayName: address.displayName,
      };
      this.profileService.addAddress(dto).subscribe({
        next: (res) => {
          if (res.success) {
            this.toaster.success(res.message);
            this.loadProfile();
            this.closeAllModals();
          } else {
            this.toaster.error(res.message);
            this.showMapModal = false;
          }
        },
        error: () => this.toaster.error(PROFILE_MESSAGES.ADDRESS.SAVE_ADDRESS_FAIL),
      });
    }
  }

  closeAllModals() {
    this.showSearchModal = false;
    this.showMapModal = false;
    this.editMode = false;
    this.editData = null;
    document.body.style.overflow = '';
  }
}
