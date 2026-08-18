import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="card form-card">
      <h1>Sign in</h1>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>
          Email
          <input type="email" formControlName="email" autocomplete="email" />
        </label>
        <label>
          Password
          <input type="password" formControlName="password" autocomplete="current-password" />
        </label>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
        <button type="submit" [disabled]="form.invalid || loading()">
          {{ loading() ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>
      <p class="muted">No account? <a routerLink="/register">Create one</a></p>
      <p class="muted">Demo: organizer&#64;example.com / attendee&#64;example.com — Password123!</p>
    </section>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly error = signal('');

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/events';
        void this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.error.set(err?.error?.message ?? 'Unable to sign in.');
        this.loading.set(false);
      }
    });
  }
}
