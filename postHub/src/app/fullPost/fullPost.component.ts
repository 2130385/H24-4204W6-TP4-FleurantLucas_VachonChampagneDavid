import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { faDownLong, faEllipsis, faImage, faMessage, faUpLong, faXmark } from '@fortawesome/free-solid-svg-icons';
import { Post } from '../models/post';
import { ActivatedRoute, Router } from '@angular/router';
import { PostService } from '../services/post.service';

@Component({
  selector: 'app-fullPost',
  templateUrl: './fullPost.component.html',
  styleUrls: ['./fullPost.component.css']
})
export class FullPostComponent implements OnInit {

  // Variables pour l'affichage ou associées à des inputs
  post : Post | null = null;
  sorting : string = "popular";
  newComment : string = "";
  newMainCommentText : string = "";
  selectedImages: File[] = [];
  imagePreviews: string[] = [];

  // Booléens sus pour cacher / afficher des boutons
  isAuthor : boolean = false;
  editMenu : boolean = false;
  displayInputFile : boolean = false;
  toggleMainCommentEdit : boolean = false;

  // Icônes Font Awesome
  faEllipsis = faEllipsis;
  faUpLong = faUpLong;
  faDownLong = faDownLong;
  faMessage = faMessage;
  faImage = faImage;
  faXmark = faXmark;

  constructor(public postService : PostService, public route : ActivatedRoute, public router : Router) { }

  async ngOnInit() {
    let postId : string | null = this.route.snapshot.paramMap.get("postId");

    if(postId != null){
      this.post = await this.postService.getPost(+postId, this.sorting);
      this.newMainCommentText = this.post.mainComment == null ? "" : this.post.mainComment.text;
    }

    
    this.isAuthor = localStorage.getItem("username") == this.post?.mainComment?.username;
  }

  onImageSelected(event: any) {
    const files: FileList = event.target.files;
    for (let i = 0; i < files.length; i++) {
      const file: File | null = files.item(i);
      if (file) {
        // Read the file as a data URL
        const reader = new FileReader();
        reader.onload = () => {
          // Add the data URL to the array
          this.imagePreviews.push(reader.result as string);
        };
        reader.readAsDataURL(file);
      }
    }
  }
  

  

  async toggleSorting(){
    if(this.post == null) return;
    this.post = await this.postService.getPost(this.post.id, this.sorting);
  }

  // Créer un commentaire directement associé au commentaire principal du post
  async createComment(){
    if(this.newComment == ""){
      alert("Écris un commentaire niochon");
      return;
    }

    const formData = new FormData();
    formData.append('text', this.newComment);
  
    for (const image of this.selectedImages) {
      formData.append('images', image);
    }

    this.post?.mainComment?.subComments?.push(await this.postService.postComment(formData, this.post.mainComment.id));

    this.newComment = "";
    this.selectedImages = [];
  }

  // Upvote le commentaire principal du post
  async upvote(){
    if(this.post == null || this.post.mainComment == null) return;
    await this.postService.upvote(this.post.mainComment.id);
    if(this.post.mainComment.upvoted){
      this.post.mainComment.upvotes -= 1;
    }
    else{
      this.post.mainComment.upvotes += 1;
    }
    this.post.mainComment.upvoted = !this.post.mainComment.upvoted;
    if(this.post.mainComment.downvoted){
      this.post.mainComment.downvoted = false;
      this.post.mainComment.downvotes -= 1;
    }
  }

  // Downvote le commentaire principal du post
  async downvote(){
    if(this.post == null || this.post.mainComment == null) return;
    await this.postService.downvote(this.post.mainComment.id);
    if(this.post.mainComment.downvoted){
      this.post.mainComment.downvotes -= 1;
    }
    else{
      this.post.mainComment.downvotes += 1;
    }
    this.post.mainComment.downvoted = !this.post.mainComment.downvoted;
    if(this.post.mainComment.upvoted){
      this.post.mainComment.upvoted = false;
      this.post.mainComment.upvotes -= 1;
    }
  }

  // Modifier le commentaire principal du post
  async editMainComment(){
    if(this.post == null || this.post.mainComment == null) return;

    let commentDTO = {
      text : this.newMainCommentText
    }

    let newMainComment = await this.postService.editComment(commentDTO, this.post?.mainComment.id);
    this.post.mainComment = newMainComment;
    this.toggleMainCommentEdit = false;
  }

  // Supprimer le commentaire principal du post. Notez que ça ne va pas supprimer le post en entier s'il y a le moindre autre commentaire.
  async deleteComment(){
    if(this.post == null || this.post.mainComment == null) return;
    await this.postService.deleteComment(this.post.mainComment.id);
    this.router.navigate(["/"]);
  }
}
