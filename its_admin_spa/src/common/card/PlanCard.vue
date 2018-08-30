<template>
  <router-link :to="detailLink" tag="div" class="fakeLink">
    <v-card style="width: 300px;">
      <v-card-media style="height: 250px">
        <img :src='photo'/>
      </v-card-media>
      <v-card-text>
        <v-layout column>
          <v-flex style="text-align: center">
            <span></span>
          </v-flex>
          <v-flex>
            <v-layout column>
              <v-flex style="text-align: start" pt-1 pb-0>
                <span class="title">{{name}}</span>
              </v-flex>
              <v-flex pt-1 pb-1>
                <span class="subheading">{{voteCount}} lượt bình chọn</span>
              </v-flex>
              <v-flex style="text-align: start" pt-1 pb-1>
                <v-label>Kéo dài {{dayCount}} ngày bởi {{creatorName}}</v-label>
              </v-flex>
            </v-layout>
          </v-flex>
        </v-layout>
      </v-card-text>
    </v-card>
  </router-link>
</template>

<script>
  export default {
    name: "PlanCard",
    props: [
      'id',
      'name',
      'photo',
      'time',
      'voter',
      'creatorName'
    ],
    computed: {
      detailLink() {
        return {name: 'PlanDetail', query: {id: this.id}};
      },
      dayCount() {
        return this.time;
      },
      voteCount() {
        return this.voter;
      }
    }
  }
</script>
