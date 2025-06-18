import {Component, OnInit} from '@angular/core';
import {StripeAccountService} from '../../../services/stripe-account.service';
import {UserService} from '../../../services/user.service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-stripe-account',
  imports: [],
  templateUrl: './stripe-account.component.html',
  styleUrl: './stripe-account.component.scss'
})
export class StripeAccountComponent implements OnInit {
  loading = false;
  isSettingUp = false;
  hasStripeAccount = false;
  accountComplete = false;
  stripeAccountId = '';
  errorMessage = '';

  constructor(
    private stripeAccountService: StripeAccountService,
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit() {
    this.checkStripeAccountStatus();
  }

  get accountStatusClass() {
    if (!this.hasStripeAccount) return 'not-configured';
    if (!this.accountComplete) return 'incomplete';
    return 'complete';
  }

  get accountStatusIcon() {
    if (!this.hasStripeAccount) return '⚠️';
    if (!this.accountComplete) return '⏳';
    return '✅';
  }

  get accountStatusTitle() {
    if (!this.hasStripeAccount) return 'Cont neconfigurat';
    if (!this.accountComplete) return 'Configurare incompletă';
    return 'Cont configurat complet';
  }

  get accountStatusDescription() {
    if (!this.hasStripeAccount) return 'Trebuie să îți configurezi contul pentru a putea primi plăți.';
    if (!this.accountComplete) return 'Completează configurarea pentru a putea primi plăți.';
    return 'Contul tău este configurat și poți primi plăți.';
  }

  async checkStripeAccountStatus() {
    this.loading = true;
    this.errorMessage = '';

    try {
      // TODO: Implement API call to check if user has Stripe account
      // For now, simulate the check
      setTimeout(() => {
        this.loading = false;
        // Set default values - you'll get these from your API
        this.hasStripeAccount = false;
        this.accountComplete = false;
      }, 1000);

    } catch (error) {
      console.error('Error checking Stripe account status:', error);
      this.errorMessage = 'Eroare la verificarea statusului contului.';
      this.loading = false;
    }
  }

  async setupStripeAccount() {
    this.isSettingUp = true;
    this.errorMessage = '';

    try {
      // Step 1: Create Stripe connected account
      const createResponse = await this.stripeAccountService.createConnectedAccount().toPromise();

      if (createResponse?.success && createResponse.response?.accountId) {
        // Step 2: Generate onboarding link
        const linkResponse = await this.stripeAccountService
          .generateOnboardingLink(createResponse.response.accountId).toPromise();

        if (linkResponse?.success && linkResponse.response?.url) {
          // Redirect to Stripe onboarding
          window.location.href = linkResponse.response.url;
        } else {
          throw new Error(linkResponse?.message || 'Eroare la generarea link-ului de configurare.');
        }
      } else {
        throw new Error(createResponse?.message || 'Eroare la crearea contului.');
      }

    } catch (error: any) {
      console.error('Error setting up Stripe account:', error);
      this.errorMessage = error.message || 'Eroare la configurarea contului Stripe.';
      this.isSettingUp = false;
    }
  }

  async continueStripeSetup() {
    this.isSettingUp = true;
    this.errorMessage = '';

    try {
      // Generate new onboarding link for existing account
      const linkResponse = await this.stripeAccountService
        .generateOnboardingLink(this.stripeAccountId).toPromise();

      if (linkResponse?.success && linkResponse.response?.url) {
        window.location.href = linkResponse.response.url;
      } else {
        throw new Error(linkResponse?.message || 'Eroare la generarea link-ului.');
      }

    } catch (error: any) {
      console.error('Error continuing Stripe setup:', error);
      this.errorMessage = error.message || 'Eroare la continuarea configurării.';
      this.isSettingUp = false;
    }
  }

  viewStripeAccount() {
    // TODO: Implement navigation to account details or Stripe Express dashboard
    console.log('View Stripe account details');
  }

  goBack() {
    this.router.navigate(['/profile']);
  }
}
