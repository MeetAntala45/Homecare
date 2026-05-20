export interface IPartnerExperience {
  companyName: string;
  role: string;
  yearsOfExperience: number;
}

export interface IPartnerLanguage {
  language: string;
}

export interface IPartnerDocument {
  documentName: string;
  filePath: string;
  fileSizeKb: number;
  fileType: string;
}

export interface IServicePartnerDetail {
  id: number;
  fullName: string;
  email: string;
  mobileNumber: string;
  serviceType: string;
  residentialAddress: string;
  statusId: number;
  totalExperienceYears: number;
  experiences: IPartnerExperience[];
  skills: string[];
  servicesOffered: string[];
  languages: IPartnerLanguage[];
  documents: IPartnerDocument[];
  averageRating: number;
  totalReviews: number;
}
