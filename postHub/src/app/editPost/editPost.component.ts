import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { faDownLong, faEllipsis, faMessage, faUpLong } from '@fortawesome/free-solid-svg-icons';
import { Hub } from '../models/hub';
import { HubService } from '../services/hub.service';
import { ActivatedRoute, Router } from '@angular/router';
import { PostService } from '../services/post.service';
import { Post } from '../models/post';

@Component({
  selector: 'app-editPost',
  templateUrl: './editPost.component.html',
  styleUrls: ['./editPost.component.css']
})
export class EditPostComponent implements OnInit {

  hub : Hub | null = null;
  postTitle : string = "";
  postText : string = "";
  @ViewChild("myPictureViewChild", {static:false}) picturesInput ?: ElementRef;

  // Icônes Font Awesome
  faEllipsis = faEllipsis;
  faUpLong = faUpLong;
  faDownLong = faDownLong;
  faMessage = faMessage;

  constructor(public hubService : HubService, public route : ActivatedRoute, public postService : PostService, public router : Router) { }

  async ngOnInit() {
    let hubId : string | null = this.route.snapshot.paramMap.get("hubId");

    if(hubId != null){
      this.hub = await this.hubService.getHub(+hubId);
    }
  }

  // Créer un nouveau post (et son commentaire principal)
  async createPost(){
    if(this.postTitle == "" || this.postText == ""){
      alert("Remplis mieux le titre et le texte niochon");
      return;
    }
    if(this.hub == null) return;

    let formData = new FormData();
    if(this.picturesInput == undefined){console.log("Input HTML non chargé")}

    let file = this.picturesInput?.nativeElement.files[0];
    if(file == null){
      console.log("Input HTML ne contient aucune image.")
    }
    formData.append("title", this.postTitle);
    formData.append("text", this.postText);
    // formData.append("image", file, file.name);
    let files = this.picturesInput?.nativeElement.files;
  if (files === null || files.length === 0) {
    console.log("Input HTML ne contient aucune image.")
  } else {
    for (let i = 0; i < files.length; i++) {
      formData.append("images", files[i], files[i].name);
    }
  }

    // let postDTO = {
    //   title : this.postTitle,
    //   text : this.postText
    // }; 

    // let newPost : Post = await this.postService.postPost(this.hub.id, postDTO);
    
    let newPost : Post = await this.postService.postPost(this.hub.id, formData);

    // On se déplace vers le nouveau post une fois qu'il est créé
    this.router.navigate(["/post", newPost.id]);
  }

}
