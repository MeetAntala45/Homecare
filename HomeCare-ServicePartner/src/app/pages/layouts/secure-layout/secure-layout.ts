import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { AdminHeader } from './admin-header/admin-header';
import { AdminSidebar } from './admin-sidebar/admin-sidebar';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PartnerSignalRService } from '../../../core/services/notifications/partner-signalr-service';
import { AiChatbot } from '../../features/ai-chatbot/ai-chatbot';

@Component({
  selector: 'app-secure-layout',
  imports: [AdminHeader, AdminSidebar, RouterOutlet, CommonModule, AiChatbot],
  templateUrl: './secure-layout.html',
  styleUrl: './secure-layout.css',
})
export class SecureLayout implements OnInit, OnDestroy {

  private readonly signalR = inject(PartnerSignalRService);

  ngOnInit(): void {
    const partnerId = parseInt(localStorage.getItem('partner_id') ?? '0');
    if (partnerId) {
      this.signalR.startConnection(partnerId);
    }
  }

  ngOnDestroy(): void {
    this.signalR.stopConnection();
  }


  sidebarOpen = false;

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
    document.body.style.overflow = 'hidden';
  }

  closeSidebar() {
    this.sidebarOpen = false;
    document.body.style.overflow = '';
  }
}
