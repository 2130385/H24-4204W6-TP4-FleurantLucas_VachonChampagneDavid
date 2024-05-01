import { Component, Input, OnInit, QueryList, ViewChildren } from "@angular/core";
import Glide from '@glidejs/glide';

@Component({
  selector: 'app-carrousel',
  templateUrl: './carrousel.component.html',
  styleUrls: ['./carrousel.component.css']
})
export class CarrouselComponent implements OnInit {

  @ViewChildren('glideitems') glideitems: QueryList<any> = new QueryList();

  @Input() imagesIds?: number[];

  constructor() { }

  ngOnInit() {
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
        perView: Math.ceil(3),
        rewind: false
      });
      glide.mount();
    }
  }

}
