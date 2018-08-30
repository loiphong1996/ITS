<template>
  <v-container id="content" fluid>
    <v-layout row pa-3 style="background-color: white" elevation-4>
      <v-flex xs12>
        <span class="title">Quản lí Yêu cầu</span>
        <v-layout row wrap>
          <v-progress-linear indeterminate v-if="pageLoading"/>
          <v-flex pa-1 sm12 md8 lg6
                  v-for="request in changeLocationRequests"
                  :key="request.requestId">
            <RequestChangeLocationInfo v-bind="request"/>
          </v-flex>
          <v-flex pa-1 sm12 md8 lg6
          v-for="request in reportReviewRequests"
          :key="request.id">
          <RequestReportReview v-bind="request"/>
          </v-flex>
        </v-layout>
        <v-layout v-if="total">
          <v-flex text-xs-center>
            <v-pagination
              v-bind="paginationModel"
              @input="paginationUpdate"
            />
          </v-flex>
        </v-layout>
      </v-flex>
    </v-layout>
    <ErrorDialog v-bind="error" v-on:close="error.dialog = false"/>
    <SuccessDialog v-bind="success" v-on:close="success.dialog = false"/>
  </v-container>
</template>

<script>
  import RequestChangeLocationInfo from "./RequestChangeLocationInfo";
  import RequestClaimOwner from "./RequestClaimOwner";
  import RequestReportReview from "./RequestReportReview";

  import {
    SuccessDialog,
    ErrorDialog
  } from "../../common/block";


  export default {
    name: "RequestListView",
    components: {
      ErrorDialog,
      SuccessDialog,
      RequestChangeLocationInfo,
      RequestClaimOwner,
      RequestReportReview
    },
    data() {
      return {
        loading:{
          changeRequests: false,
          reportReviewRequests: false
        },
        requests: undefined,
        changeLocationRequests: undefined,
        reportReviewRequests: undefined,
        filter: {
          locationChangeRequest: true,
          claimOwner: true,
          reportReview: true,
          done: false
        },
        searchInput: '',
        pagination: {
          page: 1,
          rowsPerPage: 6
        },
        total: undefined,
        //DIALOG START
        error: {
          dialog: false,
          title: '',
          message: ''
        },
        success: {
          dialog: false,
          title: '',
          message: ''
        }
        //DIALOG END;
      }
    },
    computed: {
      pageLoading(){
        return this.loading.reportReviewRequests && this.loading.changeRequests;
      },
      paginationModel() {
        const length = Math.ceil(this.total / this.pagination.rowsPerPage);
        return {
          value: this.pagination.page,
          length: length ? length : 0
        }
      }
    },
    mounted() {
      this.loadData();
    },
    methods: {
      loadData() {
        this.loading.changeRequests = true;
        this.loading.reportReviewRequests = true;

        this.$store.dispatch('request/getChangeLocationRequest')
          .then(value => {
            this.loading.changeRequests = false;
            this.changeLocationRequests = value;
          })
          .catch(reason => {
          });

        this.$store.dispatch('request/getReportReviewRequests')
          .then(value => {
            this.loading.reportReviewRequests = false;
            this.reportReviewRequests = value;
          })

      },
      onSearchEnter() {
        this.pagination.page = 1;
        this.loadData();
      },
      paginationUpdate(page) {
        this.pagination.page = page;
        this.loadData();
      }
    }
  }
</script>

<style scoped>

</style>
