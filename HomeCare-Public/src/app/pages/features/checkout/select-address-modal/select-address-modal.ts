import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ISavedAddress } from '../../../../core/models/profile/ISavedAddress';
import { ProfileService } from '../../../../core/services/profile/profile-service';

@Component({
  selector: 'app-select-address-modal',
  imports: [CommonModule],
  templateUrl: './select-address-modal.html',
  styleUrl: './select-address-modal.css'
})
export class SelectAddressModal implements OnInit {

  @Output() openInMap = new EventEmitter<ISavedAddress>();
  @Output() addNewAddress = new EventEmitter<void>();
  @Output() closeModal = new EventEmitter<void>();

  addresses: ISavedAddress[] = [];
  isLoading = false;
  selectedAddressId: number | undefined = undefined;

  constructor(private profileService: ProfileService) {}

  ngOnInit() {
    this.loadAddresses();
  }

  loadAddresses() {
    this.isLoading = true;
    this.profileService.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.addresses = res.data.addresses;
        }
        this.isLoading = false;
      },
      error: () => (this.isLoading = false)
    });
  }

  selectAddress(addr: ISavedAddress) {
    this.selectedAddressId = addr.id;
    const currentAddr = this.addresses.find(a => a.id === this.selectedAddressId);
    if (addr) this.openInMap.emit(currentAddr);
  }


  onAddNew() {
    this.addNewAddress.emit();
  }

  close() {
    this.closeModal.emit();
  }

  getFullLine(addr: ISavedAddress): string {
    const parts = [addr.houseFlatNo, addr.landmark, addr.displayName].filter(Boolean);
    return parts.join(', ');
  }
}