<template>
  <v-content id="content-holder">
    <v-layout justify-center align-center id="layout">
      <v-card id="form">
        <AppCardTitle/>
        <v-card-text>
          <v-layout column>
            <v-flex>
              <v-text-field
                label="Email"
                v-model="emailInput"
              />
              <v-text-field
                label="Mật khẩu"
                type="password"
                v-model="passwordInput"
              />
            </v-flex>
            <v-flex>
              <v-layout wrap row>
                <v-flex xs12>
                  <v-alert
                    v-model="signinAlert.show"
                    dismissible
                    type="error"
                  >
                    {{signinAlert.message}}
                  </v-alert>
                </v-flex>
                <v-flex>
                  <v-layout row wrap>
                    <v-flex xs12 class="justify-center">
                      <v-btn color="primary" block
                             :loading="loading.signinBtn"
                             @click="signin">
                        Đăng nhập
                      </v-btn>
                    </v-flex>
                    <v-flex pa-1 xs6 class="justify-center">
                      <v-btn color="secondary" block
                             :loading="recoverLoading"
                             @click="emailInputDialog = true">
                        Khôi phục mật khẩu
                      </v-btn>
                    </v-flex>
                    <v-flex pa-1 xs6 class="justify-center">
                      <v-btn color="success" block
                             :to="{name: 'Signup'}">
                        Đăng ký
                      </v-btn>
                    </v-flex>
                  </v-layout>
                </v-flex>
                <v-flex xs12>
                  <v-divider class="my-2"/>
                </v-flex>
                <!--<v-flex xs12 lg6 d-flex class="justify-center">-->
                  <!--<v-btn color="red" dark @click="signinGoogle"-->
                  <!--:loading="loading.googleBtn">-->
                    <!--<v-icon>fab fa-google</v-icon>-->
                    <!--&nbsp;&nbsp;-->
                    <!--Đăng nhập Google-->
                  <!--</v-btn>-->
                <!--</v-flex>-->
                <!--<v-flex xs12 lg6 d-flex class="justify-center">-->
                  <!--<v-btn color="primary" dark @click="signinFacebook"-->
                         <!--:loading="loading.facebookBtn">-->
                    <!--<v-icon>fab fa-facebook</v-icon>-->
                    <!--&nbsp;&nbsp;-->
                    <!--Đăng nhập Facebook-->
                  <!--</v-btn>-->
                <!--</v-flex>-->
              </v-layout>
            </v-flex>
          </v-layout>
        </v-card-text>
      </v-card>
    </v-layout>
    <!--DIALOG-->
    <v-dialog v-model="emailInputDialog" max-width="550">
      <!--RECOVER PASSWORD-->
      <v-card>
        <v-card-title class="white--text light-blue darken-2 title">
          Khôi phục mật khẩu
        </v-card-title>
        <v-card-text>
          <v-layout column>
            <v-flex>
              Chúng tôi sẽ gửi mật khẩu mới đến email mà bạn đã đăng kí
            </v-flex>
            <v-flex>
              <v-text-field label="Email" v-model="recoverEmailInput">

              </v-text-field>
            </v-flex>
          </v-layout>
        </v-card-text>
        <v-card-actions>
          <v-layout>
            <v-btn color="primary"
                   :loading="recoverLoading"
                   @click="onRecoverClick">
              Khôi phục
            </v-btn>
            <v-btn color="secondary" @click="emailInputDialog = false">
              Hủy
            </v-btn>
          </v-layout>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </v-content>
</template>

<script>
  import Raven from "raven-js";
  import {mapState, mapGetters} from "vuex";
  import {AppCardTitle} from "../../common/block";


  export default {
    name: "SigninView",
    components:{
      AppCardTitle
    },
    data() {
      return {
        loading: {
          signinBtn: false,
          googleBtn: false,
          facebookBtn: false
        },
        //RECOVER PASSWORD
        recoverEmailInput: undefined,
        emailInputDialog: false,

        //
        emailInput: undefined,
        passwordInput: undefined,

        error: {},
        signinAlert: {},
      }
    },
    computed: {
      ...mapState('account', {
        recoverLoading: state => state.loading.recoverPassword,
      }),
      ...mapGetters('authenticate',{
        isLoggedInEmail: 'isLoggedIn',
        isLoggedInFacebook: 'isLoggedInFacebook'
      }),
    },
    methods: {
      signin() {
        this.loading.signinBtn = true;
        this.$store.dispatch('authenticate/fetchToken', {
          email: this.emailInput,
          password: this.passwordInput
        })
          .then(value => {
            this.$store.commit('authenticate/setToken', {token: value});
            this.$store.dispatch('user/fetchCurrentInfo');
            if (!!this.$store.state['user/mobileToken']) {
              this.$store.dispatch('user/updateMobileToken');
            }
            const returnRoute = this.$store.getters['signinContext'].returnRoute;
            Raven.captureBreadcrumb({
              message: 'signin',
              category: 'methods',
              data: {
                returnRoute
              }
            });
            if (returnRoute) {
              this.$router.push({
                ...returnRoute
              });
            } else {
              this.$router.push({
                name: 'Home'
              });
            }
          })
          .catch(reason => {
            Raven.captureException(reason);
            this.signinAlert = {
              show: true,
              ...reason
            };
            this.loading.signinBtn = false;
          })
      },
      signinGoogle() {
        this.loading.googleBtn = true;
        this.$store.dispatch('authenticate/signinGoogle')
          .then(value => {
            const returnRoute = this.$store.getters['signinContext'].returnRoute;
            this.loading.googleBtn = false;
            Raven.captureBreadcrumb({
              message: 'signin',
              category: 'methods',
              data: {
                returnRoute
              }
            });
            if (returnRoute) {
              this.$router.push({
                ...returnRoute
              });
            } else {
              this.$router.push({
                name: 'Home'
              });
            }
          })
      },
      signinFacebook() {
        this.loading.facebookBtn = true;
        this.$store.dispatch('authenticate/signinFacebook')
          .then(value =>{
            this.loading.facebookBtn = false;
            const returnRoute = this.$store.getters['signinContext'].returnRoute;
            Raven.captureBreadcrumb({
              message: 'signin',
              category: 'methods',
              data: {
                returnRoute
              }
            });
            if (returnRoute) {
              this.$router.push({
                ...returnRoute
              });
            } else {
              this.$router.push({
                name: 'Home'
              });
            }
          })
      },
      onRecoverClick() {
        this.emailInputDialog = false;
        this.$store.dispatch('account/recoverPassword', {
          email: this.recoverEmailInput
        })
      }
    }
  }
</script>

<style scoped>
  #content-holder {
    background-image: url("../../../static/pexels-photo-239520.jpeg");
    background-size: cover;
    background-position: center center;
  }

  #layout {
    height: 100vh;
  }

  #form {
    width: 100%;
    max-width: 500px;
    /*position: relative;*/
    /*top: -15%;*/
  }
</style>
