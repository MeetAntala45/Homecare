import { Component } from '@angular/core';
import { RouterOutlet } from "@angular/router";
import { AiChatbot } from '../../features/ai-chatbot/ai-chatbot';

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet, AiChatbot],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.css',
})
export class AuthLayout {

}
