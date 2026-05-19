import { AfterViewInit, Directive, ElementRef, HostListener, Input } from '@angular/core';
import { MatTooltip } from '@angular/material/tooltip';

@Directive({
  selector: '[truncateTooltip]',
  standalone: true,
  hostDirectives: [MatTooltip],
})
export class TruncateTooltipDirective implements AfterViewInit {
  @Input() truncateTooltip = '';

  constructor(private el: ElementRef, private tooltip: MatTooltip) { }

  @HostListener('mouseenter')
  ngAfterViewInit() {
    this.checkTruncation();
  }

  public checkTruncation(): void {
    const element = this.el.nativeElement;
    const isTruncated = element.scrollWidth > element.clientWidth;
    this.tooltip.message = isTruncated ? this.truncateTooltip : '';
  }
}