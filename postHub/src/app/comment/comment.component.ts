import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { faDownLong, faEllipsis, faImage, faL, faMessage, faUpLong, faXmark } from '@fortawesome/free-solid-svg-icons';
import { Comment } from '../models/comment';
import { PostService } from '../services/post.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-comment',
  templateUrl: './comment.component.html',
  styleUrls: ['./comment.component.css']
})
export class CommentComponent implements OnInit {

  @Input() comment: Comment | null = null;
  @ViewChild("editCommentPictures", { static: false }) editCommentPictures?: ElementRef;

  // Icônes Font Awesome
  faEllipsis = faEllipsis;
  faUpLong = faUpLong;
  faDownLong = faDownLong;
  faMessage = faMessage;
  faImage = faImage;
  faXmark = faXmark;

  // Plein de variables sus pour afficher / cacher des éléments HTML
  replyToggle: boolean = false;
  editToggle: boolean = false;
  repliesToggle: boolean = false;
  isAuthor: boolean = false;
  editMenu: boolean = false;
  displayInputFile: boolean = false;

  // Variables associées à des inputs
  newComment: string = "";
  editedText?: string;
  selectedImages: File[] = [];
  commentPicturesIds: number[] = [];



  constructor(public postService: PostService) { }

  async ngOnInit() {
    this.isAuthor = localStorage.getItem("username") == this.comment?.username;
    this.editedText = this.comment?.text;
    if (this.comment?.id !== undefined) {
      this.commentPicturesIds = await this.postService.getPicturesIds(this.comment.id);
    }
  }

  onFileChange(event: any) {
    const files = event.target.files;
    if (files) {
      for (let i = 0; i < files.length; i++) {
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.selectedImages.push(e.target.result);
        };
        reader.readAsDataURL(files[i]);
        console.log(files[i]);
      }
    }
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

  // async getCommentPictures(commentId: number) {
  //   this.commentPictures = await this.postService.getCommentPictures(commentId);
  //   console.log(this.commentPictures);
  // }

  // Créer un nouveau sous-commentaire au commentaire affiché dans ce composant
  // (Pouvoir les commentaires du post, donc ceux qui sont enfant du commentaire principal du post,
  // voyez le composant fullPost !)
  async createComment() {
    if (this.newComment == "") {
      alert("Écris un commentaire niochon !");
      return;
    }

    if (this.comment == null) return;
    if (this.comment.subComments == null) this.comment.subComments = [];

    const formData = new FormData();
    formData.append('text', this.newComment);


    this.selectedImages.forEach((fileDataURL, index) => {
      const fileBlob = this.dataURItoBlob(fileDataURL);
      formData.append('files[]', fileBlob, 'file_' + index);
    });

    this.comment.subComments.push(await this.postService.postComment(formData, this.comment.id));

    this.replyToggle = false;
    this.repliesToggle = true;
    this.newComment = "";
    this.selectedImages = [];
  }

  // Modifier le texte (et éventuellement ajouter des images) d'un commentaire
  async editComment() {

    if (this.comment == null || this.editedText == undefined || this.editCommentPictures == undefined) return;

    let formData = new FormData();
    if (this.editCommentPictures == undefined) { console.log("Input HTML non chargé") }

    let file = this.editCommentPictures.nativeElement.files[0];
    if (file == null) {
      console.log("Input HTML ne contient aucune image.")
    }
    formData.append("text", this.editedText);
    // formData.append("image", file, file.name);
    let files = this.editCommentPictures.nativeElement.files;
    if (files === null || files.length === 0) {
      console.log("Input HTML ne contient aucune image.")
    } else {
      for (let i = 0; i < files.length; i++) {
        formData.append("images", files[i], files[i].name);
      }
    }

    // let commentDTO = {
    //   text: this.editedText
    // }

    let newMainComment = await this.postService.editComment(formData, this.comment.id);
    this.comment = newMainComment;
    this.editedText = this.comment.text;
    this.editMenu = false;
    this.editToggle = false;
  }

  // Supprimer un commentaire (le serveur va le soft ou le hard delete, selon la présence de sous-commentaires)
  async deleteComment() {
    if (this.comment == null || this.editedText == undefined) return;
    await this.postService.deleteComment(this.comment.id);

    // Changements visuels pour le soft-delete
    if (this.comment.subComments != null && this.comment.subComments.length > 0) {
      this.comment.username = null;
      this.comment.upvoted = false;
      this.comment.downvoted = false;
      this.comment.upvotes = 0;
      this.comment.downvotes = 0;
      this.comment.text = "Commentaire supprimé.";
      this.isAuthor = false;
    }
    // Changements ... visuels ... pour le hard-delete
    else {
      this.comment = null;
    }
  }

  // Upvoter (notez que ça annule aussi tout downvote fait pas soi-même)
  async upvote() {
    if (this.comment == null) return;
    await this.postService.upvote(this.comment.id);

    // Changements visuels immédiats
    if (this.comment.upvoted) {
      this.comment.upvotes -= 1;
    }
    else {
      this.comment.upvotes += 1;
    }
    this.comment.upvoted = !this.comment.upvoted;
    if (this.comment.downvoted) {
      this.comment.downvoted = false;
      this.comment.downvotes -= 1;
    }
  }

  // Upvoter (notez que ça annule aussi tout upvote fait pas soi-même)
  async downvote() {
    if (this.comment == null) return;
    await this.postService.downvote(this.comment.id);

    // Changements visuels immédiats
    if (this.comment.downvoted) {
      this.comment.downvotes -= 1;
    }
    else {
      this.comment.downvotes += 1;
    }
    this.comment.downvoted = !this.comment.downvoted;
    if (this.comment.upvoted) {
      this.comment.upvoted = false;
      this.comment.upvotes -= 1;
    }
  }

  async deletePicture(pictureId: number) {
    const index = this.commentPicturesIds.indexOf(pictureId);
    if (index !== -1) {
      this.postService.deletePicture(pictureId);
      this.commentPicturesIds.splice(index, 1);
    }
  }

  async reportComment() {
    const commentId : any = this.comment?.id;
    await this.postService.reportComment(commentId);
  }
}
