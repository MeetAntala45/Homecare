import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { CustomerHeader } from '../../layouts/customer-layout/customer-header/customer-header';
import { AiChatbot } from '../ai-chatbot/ai-chatbot';

@Component({
  selector: 'app-service-partner-success',
  imports: [CommonModule,MatIconModule,MatButtonModule,CustomerHeader, AiChatbot],
  templateUrl: './service-partner-success.html',
  styleUrl: './service-partner-success.css',
})
export class ServicePartnerSuccess {

  constructor(
    private router: Router
  ) {}

  onGoHomeClick(): void {
    this.router.navigate(['/customer/home']);
  }
   
  addServices() : void {
    this.router.navigate(['/customer/services']);
  }
}
