
export interface IPersonalDetail {
  profileImage: File | null;
  fullName: string;
  dateOfBirth: string;
  gender: string;
  mobileNumber: string;
  email: string;
  applyingFor: string;
  permanentAddress: string;
  residentialAddress: string;
}


export interface IEducationInfo {
  schoolCollege: string;
  passingYear: string;
  marks: string;
}


export interface IProfessionalInfo {
  companyName: string;
  role: string;
  fromDate: string;
  toDate: string;
}


export interface ILanguage {
  language: string;
  proficiency: string;
}


export interface ISkillExpertise {
  categoryId: string;
  categoryName: string;
  subCategories: string[];
}


export interface IAttachment {
  file: File;
  fileName: string;
  fileSize: number;
  fileType: string;
}


export interface IDropdownOption {
  label: string;
  value: string;
}


export interface IServicePartnerForm {
  personalDetail: IPersonalDetail;
  educationInfoList: IEducationInfo[];
  professionalInfoList: IProfessionalInfo[];
  skillExpertiseList: ISkillExpertise[];
  languageList: ILanguage[];
  attachmentList: IAttachment[];
}
export interface IServicePartnerResponse {
  id: string;
  status: 'Active' | 'Inactive';
  message: string;
  createdAt: string;
}
export interface IServiceOffered {
  subCategoryId: string;
  subCategoryName: string;
}


export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}
