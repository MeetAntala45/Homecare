import { Component, OnInit, AfterViewChecked, ViewChild, ElementRef, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IChatHistoryItem, IChatMessage } from '../../../core/models/chatbot/IChatbot';
import { ChatBotService } from '../../../core/services/chatbot/chatbot-service';
import { CHATBOT_MESSAGES } from '../../../core/constants/chatbot-messages';

const SESSION_MESSAGES_KEY = 'hc_chat_messages';
const SESSION_HISTORY_KEY = 'hc_chat_history';

@Component({
  selector: 'app-ai-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-chatbot.html',
  styleUrl: './ai-chatbot.css',
})
export class AiChatbot implements OnInit, AfterViewChecked {
  @Input() userContext = 'customer';

  @ViewChild('messagesContainer') private messagesEl!: ElementRef;
  @ViewChild('inputRef') private inputEl!: ElementRef;

  isOpen = false;
  userInput = '';
  isLoading = false;
  private doScroll = false;

  messages: IChatMessage[] = [];
  private history: IChatHistoryItem[] = [];

  private get welcomeMessage(): IChatMessage {
    return {
      role: 'bot',
      text:
        this.userContext === 'admin'
          ? CHATBOT_MESSAGES.INITIAL_ADMIN_MESSAGE
          : CHATBOT_MESSAGES.INITIAL_MESSAGE,
      time: this.now(),
    };
  }

  private get messagesKey(): string {
    return `${SESSION_MESSAGES_KEY}_${this.userContext}`;
  }

  private get historyKey(): string {
    return `${SESSION_HISTORY_KEY}_${this.userContext}`;
  }

  constructor(private chatBotService: ChatBotService) {}

  ngOnInit(): void {
    const savedMessages = this.loadMessages();
    this.messages = savedMessages.length ? savedMessages : [this.welcomeMessage];
    this.history = this.loadHistory();
    this.chatBotService.logoutReset$.subscribe(() => {
      this.resetOnLogout();
    });
  }

  ngAfterViewChecked(): void {
    if (this.doScroll) {
      this.scrollBottom();
      this.doScroll = false;
    }
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      setTimeout(() => {
        this.inputEl?.nativeElement?.focus();
        this.scrollBottom();
      }, 280);
    }
  }

  closeChat(): void {
    this.isOpen = false;
  }

  quickSend(text: string): void {
    this.userInput = text;
    this.sendMessage();
  }

  sendMessage(): void {
    const text = this.userInput.trim();
    if (!text || this.isLoading) return;

    this.messages.push({ role: 'user', text, time: this.now() });
    this.userInput = '';
    this.doScroll = true;

    this.isLoading = true;
    this.messages.push({ role: 'bot', text: '', time: this.now(), isTyping: true });
    this.doScroll = true;

    this.chatBotService
      .sendMessage({
        message: text,
        history: [...this.history],
        userContext: this.userContext,
      })
      .subscribe({
        next: (response) => {
          this.messages = this.messages.filter((m) => !m.isTyping);

          if (response?.success && response.data?.reply) {
            const reply = response.data.reply;
            this.messages.push({ role: 'bot', text: reply, time: this.now() });
            this.history.push({ role: 'user', text });
            this.history.push({ role: 'model', text: reply });
          } else {
            this.messages.push({
              role: 'bot',
              text: response?.message || CHATBOT_MESSAGES.RESET_MESSAGE,
              time: this.now(),
            });
          }

          this.saveMessages();
          this.saveHistory();
          this.isLoading = false;
          this.doScroll = true;
        },
        error: () => {
          this.messages = this.messages.filter((m) => !m.isTyping);
          this.messages.push({
            role: 'bot',
            text: CHATBOT_MESSAGES.ERROR_MESSAGE,
            time: this.now(),
          });
          this.saveMessages();
          this.isLoading = false;
          this.doScroll = true;
        },
      });
  }

  onKeyDown(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      this.sendMessage();
    }
  }

  clearChat(): void {
    this.history = [];
    this.messages = [
      {
        role: 'bot',
        text: CHATBOT_MESSAGES.RESET_MESSAGE,
        time: this.now(),
      },
    ];
    this.clearSession();
  }

  resetOnLogout(): void {
    this.isOpen = false;
    this.isLoading = false;
    this.userInput = '';
    this.history = [];
    this.messages = [this.welcomeMessage];
    this.clearSession();
  }

  private saveMessages(): void {
    const toSave = this.messages.filter((m) => !m.isTyping);
    sessionStorage.setItem(this.messagesKey, JSON.stringify(toSave));
  }

  private loadMessages(): IChatMessage[] {
    try {
      const raw = sessionStorage.getItem(this.messagesKey);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  private saveHistory(): void {
    sessionStorage.setItem(this.historyKey, JSON.stringify(this.history));
  }

  private loadHistory(): IChatHistoryItem[] {
    try {
      const raw = sessionStorage.getItem(this.historyKey);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  private clearSession(): void {
    sessionStorage.removeItem(this.messagesKey);
    sessionStorage.removeItem(this.historyKey);
  }

  private now(): string {
    return new Date().toLocaleTimeString('en-IN', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: true,
    });
  }

  private scrollBottom(): void {
    try {
      const el = this.messagesEl?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch {}
  }
}
