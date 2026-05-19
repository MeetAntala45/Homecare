import { Component } from '@angular/core';
import { CustomerHeader } from "./customer-header/customer-header";
import { RouterOutlet } from "@angular/router";
import { CustomerFooter } from "./customer-footer/customer-footer";
import { AiChatbot } from '../../features/ai-chatbot/ai-chatbot';


@Component({
  selector: 'app-customer-layout',
  imports: [CustomerHeader, RouterOutlet, CustomerFooter, AiChatbot ],
  templateUrl: './customer-layout.html',
  styleUrl: './customer-layout.css',
})
export class CustomerLayout {

}
