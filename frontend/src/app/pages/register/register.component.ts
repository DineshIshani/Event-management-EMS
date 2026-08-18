import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="card form-card">
      <h1>Create account</h1>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>
          Full name
          <input type="text" formControlName="fullName" autocomplete="name" />
        </label>
        <label>
          Email
          <input type="email" formControlName="email" autocomplete="email" />
        </label>
        <label>
          Password
          <input type="password" formControlName="password" autocomplete="new-password" />
        </label>
        <label>
          I want to
          <select formControlName="role">
            <option value="Attendee">Attend events</option>
            <option value="Organizer">Organize events</option>
          </select>
        </label>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
        <button type="submit" [disabled]="form.invalid || loading()">
          {{ loading() ? 'Creating…' : 'Create account' }}
        </button>
      </form>
      <p class="muted">Already registered? <a routerLink="/login">Sign in</a></p>
    </section>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['Attendee' as 'Attendee' | 'Organizer', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigate(['/events']),
      error: (err) => {
        this.error.set(err?.error?.message ?? 'Unable to create the account.');
        this.loading.set(false);
      }
    });
  }
}
