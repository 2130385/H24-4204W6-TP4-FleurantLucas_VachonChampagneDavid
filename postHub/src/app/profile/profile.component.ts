import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {

  userIsConnected : boolean = false;

  // Vous êtes obligés d'utiliser ces trois propriétés
  oldPassword : string = "";
  newPassword : string = "";
  newPasswordConfirm : string = "";

  username : string | null = null;

  constructor(public userService : UserService) { }

  ngOnInit() {
    this.userIsConnected = localStorage.getItem("token") != null;
    this.username = localStorage.getItem("username");
  }
  onFileSelected(event: Event): void {
    const fileInput = event.target as HTMLInputElement;
    if (fileInput.files && fileInput.files.length > 0) {
      const file: File = fileInput.files[0];
      const formData: FormData = new FormData();
      formData.append('file', file);
      this.uploadProfilePicture(formData);
    }
  }
  

  async uploadProfilePicture(formData: FormData): Promise<void> {
    try {
      await this.userService.uploadProfilePicture(this.username, formData);
      window.location.reload();
    } catch (error) {
      console.error('Erreur lors du changement de la photo de profil :', error);
    }
  }

  async changepassword() : Promise<void> {
    await this.userService.changePassword(this.oldPassword, this.newPassword, this.newPasswordConfirm);
  }
}
