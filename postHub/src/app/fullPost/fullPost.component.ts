import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { faDownLong, faEllipsis, faImage, faMessage, faUpLong, faXmark } from '@fortawesome/free-solid-svg-icons';
import { Post } from '../models/post';
import { ActivatedRoute, Router } from '@angular/router';
import { PostService } from '../services/post.service';
import { CommentComponent } from '../comment/comment.component';

@Component({
  selector: 'app-fullPost',
  templateUrl: './fullPost.component.html',
  styleUrls: ['./fullPost.component.css']
})
export class FullPostComponent implements OnInit {

  @ViewChild("myPictureViewChild", {static:false}) picturesInput ?: ElementRef;
  @ViewChild("mainCommentEditPictures", {static:false}) mainCommentEditPictures ?: ElementRef;

  // Variables pour l'affichage ou associées à des inputs
  post : Post | null = null;
  sorting : string = "popular";
  newComment : string = "";
  newMainCommentText : string = "";
  listImageIds : number[] = [];

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
      if(this.post.mainComment?.id != null)
      this.listImageIds = await this.postService.getPicturesIds(this.post.mainComment?.id);
    }

    
    this.isAuthor = localStorage.getItem("username") == this.post?.mainComment?.username;
  }

  dataURItoBlob(dataURI: any) {
    const parts = dataURI.split(';base64,');
    const contentType = parts[0].split(':')[1];
    const byteCharacters = atob(parts[1]);
  
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
  
    return new Blob([byteArray], { type: contentType });
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
    /////
    let formData = new FormData();
    if(this.picturesInput == undefined){console.log("Input HTML non chargé")}

    let file = this.picturesInput?.nativeElement.files[0];
    if(file == null){
      console.log("Input HTML ne contient aucune image.")
    }
    formData.append("text", this.newComment);
    // formData.append("image", file, file.name);
    let files = this.picturesInput?.nativeElement.files;
  if (files === null || files.length === 0) {
    console.log("Input HTML ne contient aucune image.")
  } else {
    for (let i = 0; i < files.length; i++) {
      formData.append("images", files[i], files[i].name);
    }
  }

    ////
    // const formData = new FormData();
    // formData.append('text', this.newComment);
  

    // this.selectedImages.forEach((fileDataURL, index) => {
    //   const fileBlob = this.dataURItoBlob(fileDataURL);
    //   formData.append('files[]', fileBlob, 'file_' + index);
    // });


    this.post?.mainComment?.subComments?.push(await this.postService.postComment(formData , this.post.mainComment.id));

    this.newComment = "";
    // this.selectedImages = [];
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

    let formData = new FormData();
    if(this.mainCommentEditPictures == undefined){console.log("Input HTML non chargé")}

    let file = this.mainCommentEditPictures?.nativeElement.files[0];
    if(file == null){
      console.log("Input HTML ne contient aucune image.")
    }
    formData.append("text", this.newMainCommentText);
    // formData.append("image", file, file.name);
    let files = this.mainCommentEditPictures?.nativeElement.files;
  if (files === null || files.length === 0) {
    console.log("Input HTML ne contient aucune image.")
  } else {
    for (let i = 0; i < files.length; i++) {
      formData.append("images", files[i], files[i].name);
    }
  }



    if(this.post == null || this.post.mainComment == null) return;

    // let commentDTO = {
    //   text : this.newMainCommentText
    // }

    let newMainComment = await this.postService.editComment(formData, this.post?.mainComment.id);
    this.post.mainComment = newMainComment;
    this.toggleMainCommentEdit = false; 
  }

  // Supprimer le commentaire principal du post. Notez que ça ne va pas supprimer le post en entier s'il y a le moindre autre commentaire.
  async deleteComment(){
    if(this.post == null || this.post.mainComment == null) return;
    await this.postService.deleteComment(this.post.mainComment.id);
    this.router.navigate(["/"]);
  }

  async deletePicture(pictureId: number) {
    const index = this.listImageIds.indexOf(pictureId);
    if (index !== -1) {
      this.postService.deletePicture(pictureId);
      this.listImageIds.splice(index, 1);
    }
  }

  async reportComment() {
    const commentId : any = this.post?.id;
    await this.postService.reportComment(commentId);
  }
}
