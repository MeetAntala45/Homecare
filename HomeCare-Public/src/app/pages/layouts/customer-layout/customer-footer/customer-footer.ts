import { Component } from '@angular/core';
import { Router, RouterLink } from "@angular/router";

@Component({
  selector: 'app-customer-footer',
  imports: [RouterLink],
  templateUrl: './customer-footer.html',
  styleUrl: './customer-footer.css',
})
export class CustomerFooter {
  constructor(private router: Router) { }
  onServicePartner() {
    this.router.navigate(['/customer/service-partner/onboarding']).then(() => {
      window.scrollTo(0, 0);
    });
  }

  onContactUs() {
    this.router.navigate(['/customer/contact-us']).then(() => {
      window.scrollTo(0, 0);
    });
  }
}
