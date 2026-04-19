import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideAngularModule, Mail, Lock, LogIn, Github, Twitter, Layers, ArrowRight, ShieldCheck } from 'lucide-angular';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './login.html',
})
export class Login {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  readonly Mail = Mail;
  readonly Lock = Lock;
  readonly LogIn = LogIn;
  readonly Github = Github;
  readonly Twitter = Twitter;
  readonly Layers = Layers;
  readonly ArrowRight = ArrowRight;
  readonly ShieldCheck = ShieldCheck;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/']);
          }
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Invalid username or password';
          this.isLoading = false;
        }
      });
    } else {
      this.loginForm.markAllAsTouched();
    }
  }
}
