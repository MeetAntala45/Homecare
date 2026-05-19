import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminProfile } from '../../../core/models/profile/profile.model';
import { AdminProfileService } from '../../../core/services/profile/admin-profile-service';
import { Toaster } from '../../../core/services/toaster/toaster';
import { MatDialog } from '@angular/material/dialog';
import { ContactModal } from '../../layouts/Profile-Modals/contact-modal/contact-modal';
import { PasswordModal } from '../../layouts/Profile-Modals/password-modal/password-modal';
import { ProfileStateService } from '../../../core/services/profile/profile-state-service';
import { PROFILE_MESSAGES } from '../../../core/constants/profile-messages';
import { resolveImageUrl } from '../../../core/utils/image-url.util';
import { API_BASE_URL } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-profile',
  imports: [CommonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {

  readonly MESSAGES = PROFILE_MESSAGES;

  baseUrl = API_BASE_URL;

  profile: AdminProfile = {
    name: '',
    role: '',
    mobileNumber: '',
    email: '',
    address: '',
    profileImage: ''
  };

  showChangePasswordModal = false;
  photoPreview: string = '';
  isHoveringPhoto = false;

  constructor(
    private profileService: AdminProfileService,
    private profileState: ProfileStateService,
    private toaster: Toaster,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.profileService.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.profile = res.data;
          this.photoPreview = resolveImageUrl(res.data.profileImage);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.toaster.error(this.MESSAGES.LOAD_FAILED);
      }
    });
  }

  onPhotoClick(): void {
    const input = document.getElementById('photoInput') as HTMLInputElement;
    input?.click();
  }

  onPhotoSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.toaster.error(this.MESSAGES.IMAGE.UNSUPPORTED_TYPE);
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this.toaster.error(this.MESSAGES.IMAGE.SIZE_EXCEEDED);
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      this.photoPreview = e.target?.result as string;
    };
    reader.readAsDataURL(file);

    this.profileService.uploadProfilePhoto(file).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.profile.profileImage = res.data;
          this.photoPreview = resolveImageUrl(res.data);
          this.profileState.updatePhoto(this.photoPreview);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.toaster.error(this.MESSAGES.PHOTO_UPLOAD_FAILED);
      }
    });
  }

  getAvatarText(): string {
    return this.profile.name
      ? this.profile.name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
      : 'P';
  }

  openEditContact(): void {
    const dialogRef = this.dialog.open(ContactModal, {
      width: '420px',
      disableClose: true,
      data: {
        mobileNumber: this.profile.mobileNumber,
        email: this.profile.email,
        address: this.profile.address
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.profile.mobileNumber = result.mobileNumber;
        this.profile.email = result.email;
        this.profile.address = result.address;
      }
    });
  }

  openChangePassword(): void {
    this.dialog.open(PasswordModal, { width: '420px', disableClose: true });
  }
}