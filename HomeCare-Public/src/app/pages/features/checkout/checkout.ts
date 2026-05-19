import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { tap } from 'rxjs';
import { AuthService } from '../../../core/services/auth/auth-service';
import { ProfileService } from '../../../core/services/profile/profile-service';
import { OtpStateService } from '../../../core/services/auth/otp-state-service';
import { OtpTimerService } from '../../../core/services/auth/otp-timer-service';
import { Toaster } from '../../../core/services/toaster/toaster';
import { FormModal } from '../../../shared/components/form-modal/form-modal';
import { IFormModalData } from '../../../core/models/shared/IFormModalData.model';
import { ISavedAddress } from '../../../core/models/profile/ISavedAddress';
import { IAddressRequest } from '../../../core/models/profile/IAddressRequest';
import { VerifyOtpModal } from './verify-otp-modal/verify-otp-modal';
import { LOGIN_MESSAGES } from '../../../core/constants/login-messages';
import { LocationSearchModal } from '../profile/location-search-modal/location-search-modal';
import { MapPicker } from '../profile/map-picker/map-picker';
import { SelectAddressModal } from './select-address-modal/select-address-modal';
import { CustomerHeader } from '../../layouts/customer-layout/customer-header/customer-header';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { PaymentService } from '../../../core/services/payment/payment-services';
import { PROFILE_MESSAGES } from '../../../core/constants/profile-messages';
import { ActivatedRoute, Router } from '@angular/router';
import { Slot } from './slot/slot';
import { CheckoutService } from '../../../core/services/checkout/checkoutService/checkout-service';
import { IAvailableCouponResponse } from '../../../core/models/checkout/available-coupon.response';
import { ICheckoutSummaryRequest } from '../../../core/models/checkout/checkout-summary.request';
import { IApplyCouponRequest } from '../../../core/models/checkout/apply-coupon.request';
import { PaymentSummary } from './payment-summary/payment-summary';
import { PaymentMethod } from './payment-method/payment-method';
import { Booking } from '../../../core/services/checkout/booking';
import { ISelectedSlot } from '../../../core/models/checkout/slot';
import { ISelectedPayment } from '../../../core/models/checkout/paymentMethod';
import { ICreateBookingRequest } from '../../../core/models/checkout/booking';
import { IAvailableCouponRequest } from '../../../core/models/checkout/available-coupon.request';
import { SUMMARY_MESSAGES } from '../../../core/constants/summary-messages';
import { PAYMENT_MESSAGES } from '../../../core/constants/payment-messages';
import { CouponSuccessModal } from '../coupon-success-modal/coupon-success-modal';
import { AiChatbot } from '../ai-chatbot/ai-chatbot';

type AddressModalState = 'none' | 'selectAddress' | 'locationSearch' | 'mapPicker';
type ModalState = 'none' | 'signin' | 'otp' | 'selectAddress' | 'locationSearch' | 'mapPicker';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LocationSearchModal,
    MapPicker,
    SelectAddressModal,
    CustomerHeader,
    VerifyOtpModal,
    Slot,
    PaymentMethod,
    PaymentSummary, 
    CurrencyPipe,
    AiChatbot
  ],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class Checkout implements OnInit {
  isLoggedIn = false;
  email = '';
  activeModal: ModalState = 'none';
  mapPickerMode: 'add' | 'edit' | 'select' = 'add';

  nameControl = new FormControl('', [Validators.required]);
  emailControl = new FormControl('', [Validators.required, Validators.email]);
  isSendingOtp = false;
  pendingName = '';
  pendingEmail = '';

  selectedAddress: ISavedAddress | null = null;
  editMode = false;
  editData: ISavedAddress | null = null;
  pendingLat = 20.5937;
  pendingLng = 78.9629;
  pendingAddress = '';

  addressModal: AddressModalState = 'none';

  selectedSlot: ISelectedSlot | null = null;
  isSlotOpen = false;
  serviceId!: number;

  coupons: IAvailableCouponResponse[] = [];

  selectedCouponCode: string | null = null;
  @Input() selectedSlotInput: ISelectedSlot | null = null;
  summary = {
    serviceName: '',
    servicePrice: 0,
    taxAmount: 0,
    discountAmount: 0,
    totalAmount: 0,
    taxPct: 0
  };

  isPaymentModalOpen = false;
  selectedPayment: ISelectedPayment | null = null;
  stripeMessage = PAYMENT_MESSAGES.STRIPE.PENDING;

  isBookingLoading = false;
  createdBookingId: number | null = null;
  slotConflictError: string = '';

  loading = false;
  error: string | null = null;

  bookingId!: number;

  constructor(
    private authService: AuthService,
    private profileService: ProfileService,
    private otpStateService: OtpStateService,
    private otpTimerService: OtpTimerService,
    private dialog: MatDialog,
    private toaster: Toaster,
    private paymentService: PaymentService,
    private router: Router,
    private route: ActivatedRoute,
    private bookingService: Booking,
    private checkoutService: CheckoutService,
    private cdr: ChangeDetectorRef,
  ) { }

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) {
      this.router.navigate(['/customer/home']);
      return;
    }

    this.serviceId = Number(idParam);
    this.isLoggedIn = this.authService.isLoggedIn();

    if (this.isLoggedIn) {
      this.loadProfileData();
    }

    this.loadInitialSummary();
    this.handleStripeReturn();

  }

  private handleStripeReturn(): void {
    const cancelled = this.route.snapshot.queryParamMap.get('cancelled');
    const expired = this.route.snapshot.queryParamMap.get('expired');
    const refunded = this.route.snapshot.queryParamMap.get('refunded');
    const failed = this.route.snapshot.queryParamMap.get('failed');

    if (cancelled === 'true') {
      this.toaster.error(PAYMENT_MESSAGES.STRIPE.CANCELLED);
    } else if (expired === 'true') {
      this.toaster.error(PAYMENT_MESSAGES.STRIPE.EXPIRED);
    } else if (refunded === 'true') {
      this.toaster.error(PAYMENT_MESSAGES.STRIPE.REFUNDED);
    } else if (failed === 'true') {
      this.toaster.error(PAYMENT_MESSAGES.STRIPE.FAILED);
    }

    if (cancelled || expired || refunded || failed) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: {},
        replaceUrl: true,
      });
    }
  }

  loadProfileData() {
    this.profileService.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.email = res.data.email.toLowerCase();
          const defaultAddress = res.data?.addresses;
          if (defaultAddress) {
            this.selectedAddress = res.data.addresses?.at(-1) ?? null;
          }
        }
      },
    });
  }

  openModal(modal: ModalState) {
    this.activeModal = modal;
    document.body.style.overflow = 'hidden';
  }

  closeAllModals() {
    this.activeModal = 'none';
    this.editMode = false;
    this.editData = null;
    document.body.style.overflow = '';
  }

  onSignInClick() {
    const expiresAt = localStorage.getItem(LOGIN_MESSAGES.STORAGE_KEYS.CHECKOUT_OTP_EXPIRE_AT);
    const isActive = expiresAt && Date.now() < new Date(expiresAt).getTime();

    if (this.otpStateService.isOtpRequested() && isActive) {
      this.pendingEmail =
        localStorage.getItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_EMAIL) ?? 'your email';
      this.pendingName = localStorage.getItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_NAME) ?? '';
      this.openModal('otp');
    } else {
      this.otpStateService.setOtpRequested(false);
      localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_EMAIL);
      localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_NAME);
      localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_SENT_AT);
      localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.CHECKOUT_OTP_EXPIRE_AT);
      this.openSignInModal();
    }
  }

  private openSignInModal() {
    const modalData: IFormModalData = {
      title: 'Sign In',
      fields: [
        { name: 'name', label: 'Name', type: 'text', required: true , maxLength: 100},
        { name: 'email', label: 'Email', type: 'email', required: true },
      ],
      submitLabel: 'Get OTP',
      onSubmit: ({ name, email }) => {
        const normalizedEmail = email.toLowerCase().trim();
        const userName = name.trim();

        return this.authService.sendOtp({ name: userName, email: normalizedEmail }).pipe(
          tap((res) => {
            if (res.success) {
              this.toaster.success(res.message);
              this.otpTimerService.setOtpSentTime();
              this.otpStateService.setOtpRequested(true);
              const expiresAt = new Date(Date.now() + 10 * 60 * 1000).toISOString();
              localStorage.setItem(LOGIN_MESSAGES.STORAGE_KEYS.CHECKOUT_OTP_EXPIRE_AT, expiresAt);
              localStorage.setItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_EMAIL, normalizedEmail);
              localStorage.setItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_NAME, userName);
              dialogRef.close();
              this.pendingEmail = normalizedEmail;
              this.pendingName = userName;
              this.openModal('otp');
            } else {
              this.toaster.error(res.message);
            }
          })
        );
      },
    };
    const dialogRef = this.dialog.open(FormModal, {
      data: modalData,
      width: '450px',
      disableClose: true,
    });
  }

  onOtpVerified(email: string) {
    this.isLoggedIn = true;
    this.email = email;
    this.loadProfileData();
    this.closeAllModals();
  }

  openAddressModal(state: AddressModalState) {
    this.addressModal = state;
    document.body.style.overflow = 'hidden';
  }

  closeAddressModals() {
    this.addressModal = 'none';
    this.editMode = false;
    this.editData = null;
    document.body.style.overflow = '';
  }

  onSelectAddressClick() {
    this.openAddressModal('selectAddress');
  }

  onAddressSelectedFromList(addr: ISavedAddress) {
    this.selectedAddress = addr;
    this.closeAddressModals();
  }

  onOpenAddressInMap(addr: ISavedAddress) {
    this.mapPickerMode = 'select';
    this.editMode = true;
    this.editData = addr;
    this.pendingLat = addr.latitude ?? 20.5937;
    this.pendingLng = addr.longitude ?? 78.9629;
    this.pendingAddress = addr.displayName ?? '';
    this.openAddressModal('mapPicker');
  }

  onAddNewAddress() {
    this.mapPickerMode = 'add';
    this.editMode = false;
    this.editData = null;
    this.openAddressModal('locationSearch');
  }

  onEditAddress() {
    if (!this.selectedAddress) return;
    this.mapPickerMode = 'edit';
    this.editMode = true;
    this.editData = this.selectedAddress;
    this.pendingLat = this.selectedAddress.latitude ?? 20.5937;
    this.pendingLng = this.selectedAddress.longitude ?? 78.9629;
    this.pendingAddress = this.selectedAddress.displayName ?? '';
    this.openAddressModal('mapPicker');
  }

  onLocationSelectedFromSearch(event: { lat: number; lng: number; display_name: string }) {
    this.pendingLat = event.lat;
    this.pendingLng = event.lng;
    this.pendingAddress = event.display_name;
    this.openAddressModal('mapPicker');
  }

  onChangeLocation() {
    this.openAddressModal('locationSearch');
  }

  onAddressConfirmed(address: ISavedAddress) {
    const dto: IAddressRequest = {
      houseFlatNo: address.houseFlatNo,
      landmark: address.landmark,
      label: address.label,
      latitude: address.latitude,
      longitude: address.longitude,
      displayName: address.displayName,
    };

    if (this.editMode && address.id) {
      this.profileService.editAddress(address.id, dto).subscribe({
        next: (res) => {
          if (res.success) {
            this.selectedAddress = { ...address };
            this.toaster.success(res.message);
            this.closeAddressModals();
          } else {
            this.toaster.error(res.message);
          }
        },
        error: () => this.toaster.error(PROFILE_MESSAGES.ADDRESS.UPDATE_ADDRESS_FAIL),
      });
    } else {
      this.profileService.addAddress(dto).subscribe({
        next: (res) => {
          if (res.success) {
            this.toaster.success(res.message);
            this.openAddressModal('selectAddress');
          } else {
            this.toaster.error(res.message);
          }
        },
        error: () => this.toaster.error('Failed to save address.'),
      });
    }
  }

  getAddressOneLiner(): string {
    const addr = this.selectedAddress;
    if (!addr) return '';
    return [addr.houseFlatNo, addr.landmark, addr.displayName].filter(Boolean).join(', ');
  }

  onSlotClick() {
    this.isSlotOpen = true;
  }

  onSlotConfirmed(event: ISelectedSlot | null) {
    this.slotConflictError = '';
    if (event) {
      this.selectedSlot = event;
    }
    this.loadCoupons();
    if (this.selectedCouponCode) {
      this.applyCoupon({
        couponCode: this.selectedCouponCode
      } as IAvailableCouponResponse);
    } else {
      this.loadSummary();
    }
    this.isSlotOpen = false;
  }
  onModalClosed() {
    this.isSlotOpen = false;
  }

  onPaymentConfirmed(payment: ISelectedPayment) {
    this.selectedPayment = payment;
    this.isPaymentModalOpen = false;
    document.body.style.overflow = '';
  }

  onPaymentModalClosed() {
    this.isPaymentModalOpen = false;
    document.body.style.overflow = '';
  }

  onPaymentMethodClick() {
    this.isPaymentModalOpen = true;
    document.body.style.overflow = 'hidden';
  }

  onPaymentClick() {
    if (!this.selectedAddress || !this.selectedSlot || !this.selectedPayment) {
      this.toaster.error(PAYMENT_MESSAGES.INCOMPLETE_STEPS);
      return;
    }

    this.isBookingLoading = true;

    const dto: ICreateBookingRequest = {
      serviceId: this.serviceId,
      addressId: this.selectedAddress.id!,
      couponCode: this.selectedCouponCode,
      slotDate: this.selectedSlot.date,
      slotStartTime: this.selectedSlot.startTime,
      slotEndTime: this.selectedSlot.endTime,
      paymentMethod: this.selectedPayment.method,
    };

    this.bookingService.createBooking(dto).subscribe({
      next: (res) => {
        if (res.success) {
          const bookingId = res.data.bookingId;

          if (this.selectedPayment!.method === 2) {
            this.paymentService.createCheckoutSession({ bookingId }).subscribe({
              next: (payRes) => {
                this.isBookingLoading = false;
                if (payRes.success && payRes.data?.url) {
                  setTimeout(() => {
                    window.location.href = payRes.data!.url;
                  }, 1000);
                } else {
                  this.toaster.error(payRes.message || PAYMENT_MESSAGES.STRIPE.SESSION_FAIL);
                }
              },
              error: () => {
                this.isBookingLoading = false;
                this.toaster.error(PAYMENT_MESSAGES.GENERIC_ERROR);
              },
            });
          } else {
            this.isBookingLoading = false;
            this.toaster.success(res.message);
            this.router.navigate(['/customer/booking/success'], {
              queryParams: { bookingId },
            });
          }
        } else {
          this.isBookingLoading = false;
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.isBookingLoading = false;
        this.toaster.error(PAYMENT_MESSAGES.BOOKING.CREATE_ERROR);
      },
    });
  }

  loadCoupons() {
    if (!this.selectedSlot) return;
    const req: IAvailableCouponRequest = {
      serviceId: this.serviceId,
      addressId: this.selectedAddress?.id!,
      slotDate: this.selectedSlot.date,
      slotStartTime: this.selectedSlot.startTime
    }
    this.checkoutService.getAvailableCoupons(req)
      .subscribe(res => {
        if (res.success && res.data) {
          this.coupons = res.data;
          this.cdr.detectChanges();
        } else {
          this.toaster.error(res.message);
        }
      });
  }

  applyCoupon(coupon: IAvailableCouponResponse) {
    if (!this.selectedSlot) return;
    const req: IApplyCouponRequest = {
      couponCode: coupon.couponCode,
      serviceId: this.serviceId,
      slotDate: this.selectedSlot.date,
      slotStartTime: this.selectedSlot.startTime
    };

    this.checkoutService.applyCoupon(req)
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.selectedCouponCode = res.data.couponCode;
            this.summary.discountAmount = res.data.discountAmount;
            this.loadSummary();
            this.dialog.open(CouponSuccessModal, {
              width: '360px',
              disableClose: false,
              data: {
                couponCode: res.data.couponCode,
                discountAmount: res.data.discountAmount
              }
            });
          } else {
            this.toaster.error(`Coupon is ${res.message}` || SUMMARY_MESSAGES.COUPON.APPLY_FAILED);
            this.selectedCouponCode = null;
            this.summary.discountAmount = 0;

            this.loadSummary();
          }
        }, error: (err) => {
          this.toaster.error(err?.error?.message || SUMMARY_MESSAGES.COUPON.APPLY_FAILED);
          this.selectedCouponCode = null;
          this.summary.discountAmount = 0;

          this.loadSummary();
        }
      })
  }

  onCouponApplied(coupon: IAvailableCouponResponse): void {
    this.applyCoupon(coupon);
  }

  onCouponRemoved(): void {
    this.selectedCouponCode = null;
    this.summary.discountAmount = 0;
    this.loadSummary();
  }

  loadSummary() {
    if (!this.selectedSlot) {
      this.loadInitialSummary();
      return;
    }
    const req: ICheckoutSummaryRequest = {
      serviceId: this.serviceId,
      slotDate: this.selectedSlot.date,
      slotStartTime: this.selectedSlot.startTime,
      couponCode: this.selectedCouponCode
    };

    this.checkoutService.getSummary(req)
      .subscribe(res => {
        if (res.success && res.data) {
          this.summary = {
            serviceName: res.data.serviceName,
            servicePrice: res.data.servicePrice,
            taxAmount: res.data.taxAmount,
            discountAmount: res.data.discountAmount,
            totalAmount: res.data.totalAmount,
            taxPct: res.data.taxPct
          };
        }
      });
  }


  loadInitialSummary() {

    const req: ICheckoutSummaryRequest = {
      serviceId: this.serviceId,
      slotDate: null,
      slotStartTime: null,
      couponCode: null
    };    

    this.checkoutService.getSummary(req)
      .subscribe(res => {
        if (res.success && res.data) {

          this.summary = {
            serviceName: res.data.serviceName,
            servicePrice: res.data.servicePrice,
            taxAmount: res.data.taxAmount,
            discountAmount: 0,
            totalAmount: res.data.totalAmount,
            taxPct: res.data.taxPct
          };

        }
        else {
          this.toaster.error(res.message);
        }

      });
  }

  removeCoupon(): void {
    this.selectedCouponCode = null;
    this.summary.discountAmount = 0;
    this.loadSummary();
    this.toaster.success(SUMMARY_MESSAGES.COUPON.REMOVED)
  }

}
