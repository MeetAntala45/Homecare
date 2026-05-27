import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnDestroy,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil, finalize } from 'rxjs';

import { SlotService } from '../../../../core/services/checkout/slot-service';
import { Toaster } from '../../../../core/services/toaster/toaster';

import {
  IDateAvailability,
  ISlotResponse,
  ISelectedSlot,
  IGetSlotsRequest,
} from '../../../../core/models/checkout/slot';

import { SLOT_BOOKING_MESSAGES, SESSION_CONFIG } from '../../../../core/constants/slot-messages';

@Component({
  selector: 'app-slot',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './slot.html',
  styleUrls: ['./slot.css'],
})
export class Slot implements OnDestroy, OnChanges {
  @Input() serviceId!: number;
  @Input() isOpen = false;
  @Input() preSelectedSlot: ISelectedSlot | null = null;
  @Output() slotConfirmed = new EventEmitter<ISelectedSlot | null>();
  @Output() modalClosed = new EventEmitter<void>();
  @Input() addressId!: number;

  readonly messages = SLOT_BOOKING_MESSAGES;
  sessions = [...SESSION_CONFIG];

  availableDates: IDateAvailability[] = [];
  slots: ISlotResponse[] = [];

  selectedDate: IDateAvailability | null = null;
  selectedSession: string = 'Morning';
  selectedSlot: ISlotResponse | null = null;

  isLoading = false;

  errorMessage = '';

  private destroy$ = new Subject<void>();

  constructor(private slotService: SlotService, private toaster: Toaster) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen']) {
      if (changes['isOpen'].currentValue === true) {
        document.body.style.overflow = 'hidden';
        this.resetState();
        if (this.serviceId && this.addressId) {
          this.loadDates();
        }
      } else {
        document.body.style.overflow = '';
      }
    }
    if (changes['addressId'] && this.isOpen && this.selectedDate) {
      this.loadAvailableSessions();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private resetState(): void {
    this.availableDates = [];
    this.slots = [];
    this.selectedDate = null;
    this.selectedSession = 'Morning';
    this.selectedSlot = null;
    this.errorMessage = '';
    this.isLoading = false;

    this.bookedSessionKeys = new Set<string>();
    this.sessionSlotsCache = new Map<string, ISlotResponse[]>();
  }

  loadDates(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.slotService
      .getAvailableDates(this.serviceId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.availableDates = res.data;

            if (this.preSelectedSlot) {
              const prevEntry = this.availableDates.find(
                (d) => d.date === this.preSelectedSlot!.date && d.hasSlot
              );
              if (prevEntry) {
                this.selectedDate = prevEntry;
                this.selectedSession = this.preSelectedSlot.session;
                this.loadAvailableSessions();
              } else {
                this.isLoading = false;
              }
            } else {
              const today = new Date().toISOString().split('T')[0];
              const todayEntry = this.availableDates.find((d) => d.date === today && d.hasSlot);
              if (todayEntry) {
                this.selectedDate = todayEntry;
                this.loadAvailableSessions();
              } else {
                this.isLoading = false;
              }
            }
          } else {
            this.isLoading = false;
          }
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = this.messages.errors.LOAD_DATES_FAILED;
          this.toaster.error(this.messages.errors.LOAD_DATES_FAILED);
        },
      });
  }

  private sessionSlotsCache = new Map<string, ISlotResponse[]>();

  loadSlots(): void {
    if (!this.selectedDate) return;

    const cacheKey = `${this.selectedDate.date}_${this.selectedSession}`;
    if (this.sessionSlotsCache.has(cacheKey)) {
      this.slots = this.sessionSlotsCache.get(cacheKey)!;
      this.isLoading = false;
      return;
    }

    this.slots = [];
    this.selectedSlot = null;
    this.errorMessage = '';

    const requestPayload: IGetSlotsRequest = {
      serviceId: this.serviceId,
      date: this.selectedDate.date,
      session: this.selectedSession,
      addressId: this.addressId,
    };

    this.slotService
      .getSlots(requestPayload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.slots = res.data;
            this.sessionSlotsCache.set(cacheKey, res.data);

            if (this.preSelectedSlot && !this.selectedSlot) {
              const prevSlot = this.slots.find(
                (s) =>
                  s.startTime === this.preSelectedSlot!.startTime &&
                  s.endTime === this.preSelectedSlot!.endTime &&
                  s.available
              );
              if (prevSlot) this.selectedSlot = prevSlot;
            }
          }
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = this.messages.errors.LOAD_SLOTS_FAILED;
          this.toaster.error(this.messages.errors.LOAD_SLOTS_FAILED);
        },
      });
  }
  bookedSessionKeys = new Set<string>();

  loadAvailableSessions(): void {
    if (!this.selectedDate) return;

    this.sessions = [];
    this.slots = [];

    const allSessionKeys = SESSION_CONFIG.map((s) => s.key);
    const availableSessions = new Set<string>();
    const bookedSessions = new Set<string>();
    let completed = 0;

    const today = new Date().toISOString().split('T')[0];
    const isToday = this.selectedDate.date === today;

    allSessionKeys.forEach((sessionKey) => {
      this.slotService
        .getSlots({
          serviceId: this.serviceId,
          date: this.selectedDate!.date,
          session: sessionKey,
          addressId: this.addressId,
        })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (res) => {
            if (res.success && res.data.length > 0) {
              const hasAvailable = res.data.some((s) => {
                if (!s.available) return false;
                if (isToday) {
                  const [h, m] = s.startTime.split(':').map(Number);
                  const now = new Date();
                  return !(now.getHours() > h || (now.getHours() === h && now.getMinutes() >= m));
                }
                return true;
              });

              if (hasAvailable) availableSessions.add(sessionKey);
              else bookedSessions.add(sessionKey);
            }

            completed++;

            if (completed === allSessionKeys.length) {
              if (availableSessions.size === 0) {
                const dateIndex = this.availableDates.findIndex(
                  (d) => d.date === this.selectedDate!.date
                );
                if (dateIndex !== -1) {
                  this.availableDates[dateIndex] = {
                    ...this.availableDates[dateIndex],
                    hasSlot: true,
                    allBooked: true,
                  };
                }

                this.sessions = [];
                this.slots = [];
                this.selectedDate = null;
                this.bookedSessionKeys = new Set<string>();

                const nextAvailable = this.availableDates.find((d) => d.hasSlot && !d.allBooked);
                if (nextAvailable) {
                  this.selectedDate = nextAvailable;
                  this.loadAvailableSessions(); 
                } else {
                  this.isLoading = false;
                }
                return;
              }

              this.sessions = [...SESSION_CONFIG].filter(
                (s) => availableSessions.has(s.key) || bookedSessions.has(s.key)
              );
              this.bookedSessionKeys = bookedSessions;

              if (this.sessions.length > 0) {
                const stillValid = this.sessions.some(
                  (s) => s.key === this.selectedSession && availableSessions.has(s.key)
                );
                if (!stillValid) {
                  const firstAvailable = this.sessions.find((s) => availableSessions.has(s.key));
                  this.selectedSession = firstAvailable?.key ?? this.sessions[0].key;
                }
                this.loadSlots();
              } else {
                this.isLoading = false;
              }
            }
          },
          error: () => {
            completed++;
            if (completed === allSessionKeys.length) {
              this.isLoading = false;
            }
          },
        });
    });
  }

  onDateSelect(date: IDateAvailability): void {
    if (!date.hasSlot || this.isDateFullyBooked(date)) {
      this.toaster.error(this.messages.modal.NO_SLOTS);
      return;
    }
    this.selectedDate = date;
    this.selectedSlot = null;
    this.isLoading = true; 

    this.loadAvailableSessions();
  }

  onSessionSelect(session: string): void {
    this.selectedSession = session;
    this.selectedSlot = null;
    this.isLoading = true; 

    this.loadSlots();
  }

  onSlotSelect(slot: ISlotResponse): void {
    if (!slot.available) {
      this.toaster.error(SLOT_BOOKING_MESSAGES.modal.SLOT_ALREADY_BOOKED);
      return;
    }
    this.selectedSlot = slot;
  }

  onConfirm(): void {
    if (!this.selectedDate) {
      this.toaster.error(this.messages.errors.SELECT_DATE);
      return;
    }
    if (!this.selectedSlot) {
      this.toaster.error(this.messages.errors.SELECT_SLOT);
      return;
    }

    const result: ISelectedSlot = {
      date: this.selectedDate.date,
      session: this.selectedSession,
      startTime: this.selectedSlot.startTime,
      endTime: this.selectedSlot.endTime,
      displayDate: this.formatDisplayDate(this.selectedDate.date),
      displayTime: `${this.formatTime(this.selectedSlot.startTime)} - ${this.formatTime(
        this.selectedSlot.endTime
      )}`,
    };

    this.toaster.success(SLOT_BOOKING_MESSAGES.modal.SUCCESS);
    this.slotConfirmed.emit(result);
    this.close();
  }

  onCancel(): void {
    this.slotConfirmed.emit(null);
    this.close();
  }

  close(): void {
    this.isOpen = false;
    this.modalClosed.emit();
  }

  formatDisplayDate(date: string): string {
    return new Date(date).toLocaleDateString('en-IN', {
      weekday: 'short',
      day: '2-digit',
      month: 'short',
    });
  }

  formatDateChip(date: string): { weekday: string; day: string; month: string } {
    const d = new Date(date);
    return {
      weekday: d.toLocaleDateString('en-IN', { weekday: 'short' }),
      day: d.toLocaleDateString('en-IN', { day: '2-digit' }),
      month: d.toLocaleDateString('en-IN', { month: 'short' }),
    };
  }

  formatTime(time: string): string {
    const [h, m] = time.split(':').map(Number);
    const period = h >= 12 ? 'PM' : 'AM';
    const hour = h % 12 || 12;
    return `${hour}:${m.toString().padStart(2, '0')} ${period}`;
  }

  isDateSelected(date: IDateAvailability): boolean {
    return this.selectedDate?.date === date.date;
  }

  isSlotSelected(slot: ISlotResponse): boolean {
    return (
      this.selectedSlot?.startTime === slot.startTime && this.selectedSlot?.endTime === slot.endTime
    );
  }
  isSlotExpired(slot: ISlotResponse): boolean {
    if (!this.selectedDate) return false;
    const today = new Date().toISOString().split('T')[0];
    if (this.selectedDate.date !== today) return false;

    const [h, m] = slot.startTime.split(':').map(Number);
    const now = new Date();
    return now.getHours() > h || (now.getHours() === h && now.getMinutes() >= m);
  }

  isDateFullyBooked(date: IDateAvailability): boolean {
    return date.hasSlot && !!date.allBooked;
  }

  get hasAvailableSlots(): boolean {
    return this.slots.some((s) => s.available);
  }

  get canConfirm(): boolean {
    return !!this.selectedDate && !!this.selectedSlot;
  }
}
