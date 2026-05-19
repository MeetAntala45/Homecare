import { IMyJobCalendarItem } from "../my-jobs/my-jobs";

export interface ICalendarDay {
    date: Date;
    isCurrentMonth: boolean;
    isToday: boolean;
    jobs: IMyJobCalendarItem[];
}