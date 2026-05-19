import { IService } from "./IService";

export interface IServiceView extends IService {
    images: string[];
    hasPartner?: Boolean;
  }