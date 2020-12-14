// Generated by sprotodump. DO NOT EDIT!
using System;
using Sproto;
using System.Collections.Generic;

public class Protocol : ProtocolBase {
	public static  Protocol Instance = new Protocol();
	private Protocol() {
		Protocol.SetProtocol<proto_100_scene.scene_cast_skill> (proto_100_scene.scene_cast_skill.Tag);
		Protocol.SetRequest<SprotoType.proto_100_scene.scene_cast_skill.request> (proto_100_scene.scene_cast_skill.Tag);
		Protocol.SetResponse<SprotoType.proto_100_scene.scene_cast_skill.response> (proto_100_scene.scene_cast_skill.Tag);

		Protocol.SetProtocol<proto_100_scene.scene_change_aoi_radius> (proto_100_scene.scene_change_aoi_radius.Tag);
		Protocol.SetRequest<SprotoType.proto_100_scene.scene_change_aoi_radius.request> (proto_100_scene.scene_change_aoi_radius.Tag);

		Protocol.SetProtocol<proto_100_scene.scene_enter_to> (proto_100_scene.scene_enter_to.Tag);
		Protocol.SetRequest<SprotoType.proto_100_scene.scene_enter_to.request> (proto_100_scene.scene_enter_to.Tag);
		Protocol.SetResponse<SprotoType.proto_100_scene.scene_enter_to.response> (proto_100_scene.scene_enter_to.Tag);

		Protocol.SetProtocol<proto_100_scene.scene_get_main_role_info> (proto_100_scene.scene_get_main_role_info.Tag);
		Protocol.SetResponse<SprotoType.proto_100_scene.scene_get_main_role_info.response> (proto_100_scene.scene_get_main_role_info.Tag);

		Protocol.SetProtocol<proto_100_scene.scene_get_monster_detail> (proto_100_scene.scene_get_monster_detail.Tag);
		Protocol.SetRequest<SprotoType.proto_100_scene.scene_get_monster_detail.request> (proto_100_scene.scene_get_monster_detail.Tag);
		Protocol.SetResponse<SprotoType.proto_100_scene.scene_get_monster_detail.response> (proto_100_scene.scene_get_monster_detail.Tag);

		Protocol.SetProtocol<proto_100_scene.scene_get_objs_info_change> (proto_100_scene.scene_get_objs_info_change.Tag);
		Protocol.SetRequest<SprotoType.proto_100_scene.scene_get_objs_info_change.request> (proto_100_scene.scene_get_objs_info_change.Tag);
		Protocol.SetResponse<SprotoType.proto_100_scene.scene_get_objs_info_change.response> (proto_100_scene.scene_get_objs_info_change.Tag);

		Protocol.SetProtocol<proto_100_scene.scene_get_role_look_info> (proto_100_scene.scene_get_role_look_info.Tag);
		Protocol.SetRequest<SprotoType.proto_100_scene.scene_get_role_look_info.request> (proto_100_scene.scene_get_role_look_info.Tag);
		Protocol.SetResponse<SprotoType.proto_100_scene.scene_get_role_look_info.response> (proto_100_scene.scene_get_role_look_info.Tag);

		Protocol.SetProtocol<proto_100_scene.scene_listen_fight_event> (proto_100_scene.scene_listen_fight_event.Tag);
		Protocol.SetRequest<SprotoType.proto_100_scene.scene_listen_fight_event.request> (proto_100_scene.scene_listen_fight_event.Tag);
		Protocol.SetResponse<SprotoType.proto_100_scene.scene_listen_fight_event.response> (proto_100_scene.scene_listen_fight_event.Tag);

		Protocol.SetProtocol<proto_100_scene.scene_walk> (proto_100_scene.scene_walk.Tag);
		Protocol.SetRequest<SprotoType.proto_100_scene.scene_walk.request> (proto_100_scene.scene_walk.Tag);
		Protocol.SetResponse<SprotoType.proto_100_scene.scene_walk.response> (proto_100_scene.scene_walk.Tag);

		Protocol.SetProtocol<proto_1_account.account_create_role> (proto_1_account.account_create_role.Tag);
		Protocol.SetRequest<SprotoType.proto_1_account.account_create_role.request> (proto_1_account.account_create_role.Tag);
		Protocol.SetResponse<SprotoType.proto_1_account.account_create_role.response> (proto_1_account.account_create_role.Tag);

		Protocol.SetProtocol<proto_1_account.account_delete_role> (proto_1_account.account_delete_role.Tag);
		Protocol.SetRequest<SprotoType.proto_1_account.account_delete_role.request> (proto_1_account.account_delete_role.Tag);
		Protocol.SetResponse<SprotoType.proto_1_account.account_delete_role.response> (proto_1_account.account_delete_role.Tag);

		Protocol.SetProtocol<proto_1_account.account_get_role_list> (proto_1_account.account_get_role_list.Tag);
		Protocol.SetResponse<SprotoType.proto_1_account.account_get_role_list.response> (proto_1_account.account_get_role_list.Tag);

		Protocol.SetProtocol<proto_1_account.account_get_server_time> (proto_1_account.account_get_server_time.Tag);
		Protocol.SetResponse<SprotoType.proto_1_account.account_get_server_time.response> (proto_1_account.account_get_server_time.Tag);

		Protocol.SetProtocol<proto_1_account.account_select_role_enter_game> (proto_1_account.account_select_role_enter_game.Tag);
		Protocol.SetRequest<SprotoType.proto_1_account.account_select_role_enter_game.request> (proto_1_account.account_select_role_enter_game.Tag);
		Protocol.SetResponse<SprotoType.proto_1_account.account_select_role_enter_game.response> (proto_1_account.account_select_role_enter_game.Tag);

	}

	public class proto_100_scene {
		public class scene_cast_skill {
			public const int Tag = 105;
		}

		public class scene_change_aoi_radius {
			public const int Tag = 103;
		}

		public class scene_enter_to {
			public const int Tag = 108;
		}

		public class scene_get_main_role_info {
			public const int Tag = 100;
		}

		public class scene_get_monster_detail {
			public const int Tag = 107;
		}

		public class scene_get_objs_info_change {
			public const int Tag = 102;
		}

		public class scene_get_role_look_info {
			public const int Tag = 104;
		}

		public class scene_listen_fight_event {
			public const int Tag = 106;
		}

		public class scene_walk {
			public const int Tag = 101;
		}

	}

	public class proto_1_account {
		public class account_create_role {
			public const int Tag = 3;
		}

		public class account_delete_role {
			public const int Tag = 4;
		}

		public class account_get_role_list {
			public const int Tag = 2;
		}

		public class account_get_server_time {
			public const int Tag = 1;
		}

		public class account_select_role_enter_game {
			public const int Tag = 5;
		}

	}

}