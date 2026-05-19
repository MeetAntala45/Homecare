export interface IChatMessage {
  role: 'user' | 'bot';
  text: string;
  time: string;
  isTyping?: boolean;
}

export interface IChatHistoryItem {
  role: string;
  text: string;
}

export interface IChatRequest {
  message: string;
  history: IChatHistoryItem[];
  userContext: string; 
}

export interface IChatResponse {
  reply: string;
}
