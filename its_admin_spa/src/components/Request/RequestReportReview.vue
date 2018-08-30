<template>
  <Request :user="user"
           title="Báo cáo bình luận"
           :status="status">
    <template slot="detail">
      <v-flex class="text-xs-center subheading">
        Cho <router-link :to="{name: 'LocationEdit', query:{id: review.locationId}}">{{review.locationName}}</router-link>
      </v-flex>
      <v-layout column>
        <v-flex>
          <v-textarea :value="content" label="Lời nhắn" readonly>

          </v-textarea>
        </v-flex>
        <v-flex>
          <LocationReview v-bind="review"/>
        </v-flex>
      </v-layout>

    </template>
  </Request>
</template>

<script>
  import Request from "./Request";
  import {LocationReview} from "../../common/block";


  export default {
    name: "RequestReportReview",
    components: {
      Request,
      LocationReview
    },
    props: [
      'isOwner',
      'title',
      'user',
      'location',
      'status',
      'review',
      'content',
      'locationId',
      'locationName'
    ]
  }
</script>

<style scoped>

</style>
