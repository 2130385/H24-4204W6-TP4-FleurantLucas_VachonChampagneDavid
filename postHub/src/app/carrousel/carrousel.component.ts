import { Component, Input, OnInit, QueryList, ViewChildren } from "@angular/core";
import Glide from '@glidejs/glide';
import { PostService } from "../services/post.service";

@Component({
  selector: 'app-carrousel',
  templateUrl: './carrousel.component.html',
  styleUrls: ['./carrousel.component.css']
})
export class CarrouselComponent implements OnInit {

  @ViewChildren('glideitems') glideitems: QueryList<any> = new QueryList();

  @Input() imagesIds?: number[];
  isAuthor : boolean = false;

  constructor(public postService : PostService) { }

  ngOnInit() {
    if(localStorage.getItem("username")){
      this.isAuthor = true;
    }
  }

  ngAfterViewInit() {
    this.glideitems.changes.subscribe(e => {
      this.initGlide();
    });
    if (this.glideitems.length > 0) {
      this.initGlide();
    }
  }


  initGlide() {
    if (this.imagesIds != undefined) {
      var glide = new Glide('.glide', {
        type: 'carousel',
        focusAt: 'center',
        perView: Math.ceil(4),
        rewind: false
      });
      glide.mount();
    }
  }

  async deletePicture(pictureId: number) {
    if(this.imagesIds?.length  !== undefined){
      const index : number = this.imagesIds.indexOf(pictureId);
      if (index !== -1) {
        this.postService.deletePicture(pictureId);
        this.imagesIds.splice(index, 1);
      }
    }
  }
}
