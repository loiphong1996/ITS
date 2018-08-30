<template>
  <v-content>
    <v-container fluid pa-0
                 v-if="!pageLoading">
      <!--<ParallaxHeader :src="location.primaryPhoto" height="400"/>-->
      <!--width: 100%; height: 500px-->
      <div
        :style="{'width': '100%', 'height':'500px', 'background-image': `url(${location.primaryPhoto})`,'background-size': 'cover'}">

      </div>
      <v-layout column>
        <v-flex mx-2 my-2>
          <!--Header-->
          <div class="headline font-weight-thin">{{location.name}}</div>
          <StarRating v-model="location.rating"
                      read-only
                      :star-size="20"
                      :increment="0.1"
                      :show-rating="false"/>
          <span class="subheading">{{location.ratingCount}} đánh giá</span>
          <div v-if="summaryTag">
            <v-chip v-for="(tag, index) in summaryTag"
                    :key="`t${index}`">{{tag.name}}
            </v-chip>
          </div>
          <div v-if="todayHours">
            <span>Hôm nay:{{todayHours.displayString}}</span>

            <span class="subheading font-weight-bold"
                  v-if="todayHours.isOpen"
                  style="color: green">
                Đang mở cửa
            </span>
            <span class="subheading font-weight-bold"
                  v-else
                  style="color: red">
              Đang đóng cửa
            </span>
          </div>
          <v-container v-if="location.description" fluid px-0>
            <v-textarea label="Mô tả" :value="location.description" disabled>
            </v-textarea>
          </v-container>
        </v-flex>
        <v-flex my-4 mx-2>
          <v-layout align-baseline>
            <v-flex class="title">Hình ảnh</v-flex>
            <v-btn v-if="isLoggedIn"
                   color="success"
                   :loading="loading.addImgBtn"
                   @click="onAddPhotoClick">
              <v-icon>add_a_photo</v-icon>
              &nbsp; &nbsp;
              Thêm ảnh
            </v-btn>
          </v-layout>
          <v-flex my-2>
            <v-carousel>
              <v-carousel-item
                v-for="(photo,i) in location.otherPhotos"
                :key="i"
                :src="photo"/>
            </v-carousel>
          </v-flex>
        </v-flex>
        <v-flex my-2 mx-2>
          <div class="title">Thông tin liên lạc</div>
          <v-layout column mx-3 my-2>
            <v-text-field
              label="Địa chỉ"
              disabled
              v-model="location.address"
              prepend-icon="map"
            />
            <v-text-field
              label="Email"
              disabled
              v-model="location.emailAddress"
              prepend-icon="email"
            />
            <v-text-field
              label="Điện thoại"
              disabled
              v-model="location.phoneNumber"
              prepend-icon="call"
            />
            <v-text-field
              label="Web"
              disabled
              v-model="location.website"
              prepend-icon="screen_share"
            />
          </v-layout>
        </v-flex>
        <v-flex my-2 mx-2>
          <v-layout align-baseline>
            <v-flex class="title">Bình luận</v-flex>
            <v-btn color="success" :to="{name:'ReviewWrite', query:{id: location.id, name: location.name}}">
              <v-icon>rate_review</v-icon>
              &nbsp &nbsp Bình luận
            </v-btn>
          </v-layout>
          <v-layout v-if="location.comments && location.comments.length > 0"
                    column my-2>
            <v-flex v-for="comment in location.comments"
                    :key="comment.id"
                    elevation-2>
              <LocationReview v-bind="comment" @report="$router.push({name: 'ReviewReport', query:{id:comment.id}})"/>
            </v-flex>
          </v-layout>
          <v-layout v-else row my-2 justify-center class="subheading">
            <span>Chưa có bình luận nào</span>
          </v-layout>
        </v-flex>
        <v-flex my-2 mx-2>
          <v-flex class="title">Địa điểm lân cận</v-flex>
          <v-layout column my-2>
            <v-divider></v-divider>
            <v-flex>
              <v-btn block flat :to="nearbyOnMapLink">
                <v-icon>navigation</v-icon>
                &nbsp &nbsp Xem trên bản đồ
              </v-btn>
            </v-flex>
            <v-divider></v-divider>
            <v-flex>
              <v-btn block flat :to="nearbyLink">
                <v-icon>far fa-building</v-icon>
                &nbsp &nbsp Các địa điểm gần đây
              </v-btn>
            </v-flex>
            <v-divider></v-divider>
          </v-layout>
        </v-flex>
        <!--<v-flex my-2 mx-2>-->
          <!--<v-flex class="title">Khác</v-flex>-->
          <!--<v-layout column my-2>-->
            <!--<v-divider></v-divider>-->
            <!--<v-flex>-->
              <!--<v-btn block flat :to="{name:'LocationChangeRequest'}">-->
                <!--<v-icon>far fa-edit</v-icon>-->
                <!--&nbsp; &nbsp; Cải thiện hồ sơ-->
              <!--</v-btn>-->
            <!--</v-flex>-->
            <!--<v-divider></v-divider>-->
          <!--</v-layout>-->
        <!--</v-flex>-->

        <v-flex style="height: 25vh">
          <!--Holder-->
        </v-flex>
      </v-layout>
      <ChooseImageDialog v-bind="chooseImageDialog"
                         @close="chooseImageDialog.dialog = false"
                         @confirm="onAddImageConfirm"/>
    </v-container>
    <v-container v-else class="text-xs-center">
      <v-progress-circular indeterminate size="40" color="primary"></v-progress-circular>
    </v-container>
  </v-content>
</template>

<script>
  import moment from "moment";
  import _ from "lodash"
  import StarRating from "vue-star-rating";
  import LocationReview from "../../common/block/LocationReview";
  import ParallaxHeader from "../../common/layout/ParallaxHeader";
  import ChooseImageDialog from "../../common/input/ChooseImageDialog";
  import {mapGetters} from "vuex";


  export default {
    name: "LocationDetailView",
    components: {
      StarRating,
      LocationReview,
      ParallaxHeader,
      ChooseImageDialog
    },
    created() {
      const {
        id
      } = this.$route.query;

      this.locationId = id;
    },
    data() {
      return {
        loading: {
          addImgBtn: false
        },
        locationId: undefined,
        chooseImageDialog: {},
        error: {}
      }
    },
    computed: {
      ...mapGetters('location', {
        location: 'detailedLocation',
        pageLoading: 'detailedLocationLoading',
      }),
      ...mapGetters('authenticate', {
        isLoggedIn: 'isLoggedIn'
      }),
      summaryTag() {
        return this.location.tags;
      },
      todayHours() {
        if (!this.location.businessHours) {
          return;
        }

        let todayHour = _(this.location.businessHours)
          .map(businessHour => {
            const fromTime = moment(businessHour.from, "HH:mm");
            const toTime = moment(businessHour.to, "HH:mm");
            const zeroTime = moment("00:01", "HH:mm");
            let day;
            switch (businessHour.day) {
              case 'day1':
                day = moment().day(0);
                break;
              case 'day2':
                day = moment().day(1);
                break;
              case 'day3':
                day = moment().day(2);
                break;
              case 'day4':
                day = moment().day(3);
                break;
              case 'day5':
                day = moment().day(4);
                break;
              case 'day6':
                day = moment().day(5);
                break;
              case 'day7':
                day = moment().day(6);
                break;
            }
            let now = moment();
            let from = moment(day)
              .set('hour', fromTime.get('hour'))
              .set('minute', fromTime.get('minute'))
              .set('second', 0);

            let to = moment(day)
              .set('hour', toTime.get('hour'))
              .set('minute', toTime.get('minute'))
              .set('second', 0);


            let formattedBusinessHour = {
              from,
              to,
              day
            };

            if (from.isSame(zeroTime, "hour") && to.isSame(zeroTime, "hour")) {
              formattedBusinessHour.isNow = true;
              formattedBusinessHour.displayString = `Mở cả ngày`;
            } else {
              formattedBusinessHour.isNow = now.isBetween(
                formattedBusinessHour.from,
                formattedBusinessHour.to,
              );
              formattedBusinessHour.displayString = `Mở từ ${from.format('LT')} đến ${to.format('LT')}`;
            }

            return formattedBusinessHour;
          })
          .filter(formattedBusinessHour => {
            const now = moment();
            return now.isSame(formattedBusinessHour.day, 'day');
          })
          .head();

        return {
          from: todayHour.from.format('HH:mm'),
          to: todayHour.to.format('HH:mm'),
          isOpen: todayHour.isNow,
          displayString: todayHour.displayString
        }
      },
      nearbyLink() {
        return {
          name: "LocationNearbyList",
          query: {
            long: this.location.long,
            lat: this.location.lat,
            title: `Các địa điểm gần ${this.location.name}`,
          }
        }
      },
      nearbyOnMapLink() {
        return {
          name: "NearbyOnMap",
          query: {
            long: this.location.long,
            lat: this.location.lat,
            locationId: this.locationId
          }
        }
      }
    },
    mounted() {
      this.$store.dispatch('location/getDetails', {
        id: this.locationId
      })
    },
    methods: {
      onAddPhotoClick() {
        this.chooseImageDialog = {
          dialog: true,
          text: 'Thêm ảnh cho địa điểm'
        }
      },
      onAddImageConfirm(photo) {
        this.chooseImageDialog = {
          dialog: false
        };

        if (photo) {
          this.$store.dispatch('location/addImage', {photo, locationId: this.location.id})
            .then(location => {
              // this.location = location;
              window.location.reload();
            })
            .catch(reason => {
              this.error = {
                dialog: true,
                ...reason
              }
            })

        }
      }
    }
  }
</script>

<style scoped>

</style>
