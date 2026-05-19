import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ISelectedPayment } from '../../../../core/models/checkout/paymentMethod';


@Component({
  selector: 'app-payment-method',
  imports: [CommonModule],
  templateUrl: './payment-method.html',
  styleUrl: './payment-method.css',
})

export class PaymentMethod implements OnChanges {
  @Input() isOpen = false;
  @Input() initialMethod: number | null = null;
  @Output() paymentConfirmed = new EventEmitter<ISelectedPayment>();
  @Output() modalClosed = new EventEmitter<void>();


  selectedMethod: number | null = null;

  ngOnChanges(changes: SimpleChanges): void {

    if (changes['isOpen']?.currentValue === true) {
      this.selectedMethod = this.initialMethod ?? null;
    }
  }

  selectMethod(method: number): void {
    this.selectedMethod = method;
  }

  onSave(): void {
    if (!this.selectedMethod) return;

    const label = this.selectedMethod === 1 ? 'Cash' : 'DebitCard';

    this.paymentConfirmed.emit({
      method: this.selectedMethod,
      label,
    });

    this.close();
  }

  onCancel(): void {
    this.modalClosed.emit();
    this.close();
  }

  close(): void {
    this.isOpen = false;
    this.modalClosed.emit();
  }
}