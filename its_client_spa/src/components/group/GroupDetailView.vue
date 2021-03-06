<template>
  <v-content>
    <v-container class="text-xs-center" v-if="pageLoading">
      <v-progress-circular indeterminate size="40" color="primary"></v-progress-circular>
    </v-container>
    <v-container v-else fluid pa-0 px-0>
      <v-toolbar dark flat color="light-blue darken-2" dense>
        <v-toolbar-title>
          {{group.name}}
        </v-toolbar-title>
        <v-tabs
          fixed-tabs
          slot="extension"
          color="light-blue darken-2"
          slider-color="white"
          v-model="tab"
          grow
        >
          <v-tab touchless key="members">
            <v-flex class="text-md-center" ma-3>
              <v-icon>fas fa-user-friends</v-icon>
              Thành viên
            </v-flex>
          </v-tab>
          <v-tab touchless key="trips">
            <v-flex class="text-md-center" xs12 my-2>
              <v-icon>fas fa-suitcase</v-icon>
              Chuyến đi
            </v-flex>
          </v-tab>
        </v-tabs>
      </v-toolbar>
      <v-tabs-items v-model="tab">
        <v-tab-item key="members">
          <v-flex xs12 px-2 py-1 class="grey lighten-4">
            <!--USERS-->
            <v-layout column>
              <v-flex v-if="group.isOwner">
                <v-btn color="success"
                       :to="inviteLink">
                  <v-icon>fas fa-user-plus</v-icon>
                  &nbsp;
                  Mời thành viên
                </v-btn>
              </v-flex>
              <v-layout row wrap my-2>
                <v-flex xs6 sm4 lg2 pa-1
                        v-for="account in group.users"
                        :key="account.id">
                  <AccountCard v-bind="account">
                    <template slot="actionBtn">
                      <v-icon small
                              v-if="currentUser.id != account.id && group.isOwner"
                              @click="onDeleteUser(account.id)"
                              color="red"
                              class="fakeLink"
                              style="position: absolute;top:5%; right: 2%">
                        fas fa-user-minus
                      </v-icon>
                    </template>
                  </AccountCard>
                </v-flex>
              </v-layout>
            </v-layout>
          </v-flex>
        </v-tab-item>
        <v-tab-item key="trips">
          <v-flex xs12 px-2 py-1 class="grey lighten-4">
            <v-layout column>
              <v-flex v-if="group.isOwner">
                <v-btn color="success"
                       :loading="addLoading"
                       @click="dialog.choosePlan = true">
                  <v-icon>fas fa-user-plus</v-icon>
                  &nbsp;
                  Thêm chuyến đi
                </v-btn>
              </v-flex>
            </v-layout>
            <!--GROUPS-->
            <v-layout column>
              <v-flex xs12 lg6 my-2 pa-2
                      class="white"
                      v-for="plan in group.plans"
                      :key="plan.id">
                <PlanFullWidth v-bind="plan"
                               @delete="onDeletePlan"
                               @save="dialog.choosePlanDestination = true"/>
              </v-flex>
            </v-layout>
          </v-flex>
        </v-tab-item>
      </v-tabs-items>
    </v-container>
    <v-flex style="height: 15vh">
      <!--Holder-->
    </v-flex>

    <ChoosePlanDestinationDialog :dialog="dialog.choosePlanDestination"
                                 @select="dialog.choosePlanDestination = false"
                                 @close="dialog.choosePlanDestination = false"
    />
    <ChoosePlanDialog
      :currentGroup="group"
      :dialog="dialog.choosePlan"
      @select="onChoosePlanSelect"
      @close="dialog.choosePlan = false"
    />
  </v-content>
</template>

<script>
  import {PlanFullWidth} from "../../common/block";
  import AccountCard from "../../common/card/AccountCard";
  import {
    ChoosePlanDestinationDialog,
    MessageInputDialog
  } from "../../common/input";
  import ChoosePlanDialog from "../../common/input/ChoosePlanDialog";
  import {mapGetters, mapState} from "vuex";

  export default {
    name: "GroupFullWidth",
    components: {
      AccountCard,
      PlanFullWidth,
      ChoosePlanDialog,
      ChoosePlanDestinationDialog,
    },
    data() {
      return {
        tab: undefined,

        dialog: {
          choosePlanDestination: false,
          choosePlan: false,
        },
        groupId: undefined,
      }
    },
    computed: {
      ...mapGetters('group', {
        group: 'detailedGroup',
        groupDetailLoading: 'detailedGroupLoading'
      }),
      ...mapState('plan', {
        deleteLoading: (state) => state.loading.delete
      }),
      ...mapState('group', {
        addLoading: (state) => state.loading.addPlanToGroup || state.loading.updateDetailedGroup
      }),
      ...mapState('user', {
        currentUser: 'current'
      }),
      pageLoading() {
        return this.groupDetailLoading && !!this.currentUser
      },
      inviteLink() {
        return {
          name: 'GroupInvite',
          query: {
            groupId: this.groupId,
            groupName: this.group.name
          }
        }
      },
    },
    created() {
      const {
        id
      } = this.$route.query;

      this.groupId = id;
    },
    mounted() {
      this.onLoad();
    },
    methods: {
      onLoad() {
        this.$store.dispatch('group/fetchById', {id: this.groupId});
        this.$store.dispatch('user/fetchCurrentInfo')
      },
      onChoosePlanSelect(plan) {
        this.$store.commit('group/setLoading', {
          loading: {updateDetailedGroup: true}
        });
        this.$store.dispatch('group/addPlanToGroup', {
          planId: plan.id,
          groupId: this.groupId
        }).then(() => {
          this.$store.dispatch('group/updateDetailedGroup');
        });
        this.dialog.choosePlan = false;
      },
      onDeletePlan(id) {
        this.$store.dispatch('plan/delete', {id});
        this.$store.commit('group/deleteGroupPlan', {id});
      },
      onDeleteUser(id) {
        this.$store.dispatch('group/deleteUser', {
          groupId: this.groupId,
          userId: id
        });
        this.$store.commit('group/deleteUser', {id});
      }
    }
  }
</script>

<style scoped>

</style>
