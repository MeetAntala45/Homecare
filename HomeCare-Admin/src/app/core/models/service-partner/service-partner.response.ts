export interface IServicePartnerListItem {
  id: number;
  servicePartnerId: string;
  fullName: string;
  mobileNumber: string;
  email: string;
  residentialAddress: string | null;
  serviceType: string;
  jobsDone: number | null;
  statusId: number;
  status: string;
}

export interface IServicePartnerListResponse {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  data: IServicePartnerListItem[];
}

export interface IPartnerEducation {
  id: number;
  instituteName: string;
  passingYear: number;
  marksPercentage: number;
}

export interface IPartnerExperience {
  id: number;
  companyName: string;
  role: string;
  fromDate: string;
  toDate: string | null;
}

export interface IPartnerLanguage {
  id: number;
  languageId: number;
  language: string;
  proficiencyId: number;
  proficiency: string;
}

export interface IPartnerDocument {
  id: number;
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
  profileImage: string | null;
  dateOfBirth: string;
  genderId: number;
  gender: string;
  serviceTypeId: number;
  permanentAddress: string;
  residentialAddress: string;
  statusId: number;
  status: string;
  educations: IPartnerEducation[];
  experiences: IPartnerExperience[];
  skillCategoryIds: number[];
  serviceOfferedIds: number[];
  languages: IPartnerLanguage[];
  documents: IPartnerDocument[];
}
