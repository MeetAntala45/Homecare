import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IMyJobCalendarItem } from '../../../core/models/my-jobs/my-jobs';
import { ICalendarDay } from '../../../core/models/shared-components/ICalendarDay';

@Component({
    selector: 'app-jobs-calendar',
    imports: [CommonModule],
    templateUrl: './jobs-calendar.html',
    styleUrl: './jobs-calendar.css'
})
export class JobsCalendar implements OnChanges {
    @Input() jobs: IMyJobCalendarItem[] = [];
    @Input() currentYear: number = new Date().getFullYear();
    @Input() currentMonth: number = new Date().getMonth() + 1;
    @Input() scrollToTodayTrigger: number = 0;

    weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    calendarDays: ICalendarDay[] = [];
    today = new Date();

    ngOnChanges(changes: SimpleChanges): void {
      if (changes['jobs'] || changes['currentMonth'] || changes['currentYear']) {
        this.buildCalendar();
      }

      if (changes['scrollToTodayTrigger']) {
        const current = changes['scrollToTodayTrigger'].currentValue;      
        const isFirstChange = changes['scrollToTodayTrigger'].firstChange;
        if (!isFirstChange && current > 0) {
          setTimeout(() => this.scrollToToday(), 100);
        }
      }
    }

    private scrollToToday(): void {
      const todayStr = this.toDateOnlyStr(this.today);
      const el = document.getElementById(`day-${todayStr}`);
    
      if (el) {
        el.scrollIntoView({
          behavior: 'smooth',
          block: 'center'
        });
      }
    }

    private getWeekStartOffset(date: Date): number {
      const actualFirstDay = date.getDay();
      const setFirstDayOfWeek = 1;
      return ( actualFirstDay - setFirstDayOfWeek + 7) % 7;
    }

    get monthLabel(): string {
        return new Date(this.currentYear, this.currentMonth - 1, 1)
            .toLocaleString('default', { month: 'long', year: 'numeric' });
    }

    get monthName(): string {
      return this.monthLabel.split(' ')[0];
    }
    
    private buildCalendar(): void {
        const firstDay = new Date(this.currentYear, this.currentMonth - 1, 1);      
        const lastDay = new Date(this.currentYear, this.currentMonth, 0);
        const days: ICalendarDay[] = [];

        const offset = this.getWeekStartOffset(firstDay);

        for (let i = 0; i < offset; i++) {
            const previousMonthDate = new Date(firstDay);
            previousMonthDate.setDate(previousMonthDate.getDate() - (offset - i));
            days.push({ date: previousMonthDate, isCurrentMonth: false, isToday: false, jobs: [] });
        }

        for (let d = 1; d <= lastDay.getDate(); d++) {
            const date = new Date(this.currentYear, this.currentMonth - 1, d);
            const dateStr = this.toDateOnlyStr(date);
            const statusPriority: any = {
              'InProgress': 1,
              'Pending': 2,
              'Completed': 3
            };
            const dayJobs = this.jobs
                .filter(j => j.bookingDate === dateStr)
                .sort((a, b) => {
                  const priorityA = statusPriority[a.bookingStatus] ?? 99;
                  const priorityB = statusPriority[b.bookingStatus] ?? 99;

                  if (priorityA !== priorityB) {
                    return priorityA - priorityB;
                  }

                  return a.slotTime.localeCompare(b.slotTime);
                });

            days.push({
                date,
                isCurrentMonth: true,
                isToday: this.isSameDay(date, this.today),
                jobs: dayJobs
            });
        }

        const remaining = 7 - (days.length % 7);
        if (remaining < 7) {
            for (let i = 1; i <= remaining; i++) {
                const nextMonthDate = new Date(lastDay);
                nextMonthDate.setDate(nextMonthDate.getDate() + i);
                days.push({ date: nextMonthDate, isCurrentMonth: false, isToday: false, jobs: [] });
            }
        }

        this.calendarDays = days;
    }

    public toDateOnlyStr(date: Date): string {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    private isSameDay(a: Date, b: Date): boolean {
        return a.getFullYear() === b.getFullYear()
            && a.getMonth() === b.getMonth()
            && a.getDate() === b.getDate();
    }

    formatTime(slotTime: string): string {
        const [h, m] = slotTime.split(':').map(Number);
        const period = h >= 12 ? 'PM' : 'AM';
        const hour = h % 12 || 12;
        return `${hour}:${String(m).padStart(2, '0')} ${period}`;
    }
}