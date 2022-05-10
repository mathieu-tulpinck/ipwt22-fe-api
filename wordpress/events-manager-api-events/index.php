<?php

/**
 * Plugin Name: Events manager api - events
 * Add REST API support to an already registered post type.
 */
add_filter('register_post_type_args', 'my_post_type_args', 10, 2);

function my_post_type_args($args, $post_type) {

    if ('event' === $post_type) {
        $args['show_in_rest'] = true;

        // Optionally customize the rest_base or rest_controller_class
        $args['rest_base'] = 'events';
        $args['rest_controller_class'] = 'WP_REST_Posts_Controller';
    }

    return $args;
}

/**
 *  Expose the specific events data.
 */

add_action('init', 'register_new_meta');

function register_new_meta() {
     $args = array(
        'type' => 'string',
        'description' => '',
        'single' => true,
        'auth_callback' => 'my_auth_callback',
        'show_in_rest' => true,

    );

    function my_auth_callback($false, $meta_key, $post_id, $user_id, $cap, $caps) {
        if ($user_id === 1) {
            return true;
        } else {
            return false;
        }
    }

    // entity_version, title, start_date, end_date, description, organiser_uuid (owner), is_active. To check in xml templates. 1 = published, null = draft, -1 = 
    $fields = ['_event_id','_event_start_time','_event_end_time','_event_start_date','_event_end_date'];

    foreach ($fields as $key => $value)
    {
         register_meta('post', $value, $args );   
    }
}

/**
 * Enables the Excerpt meta box in Page edit screen, this allows the information
 * to appear in REST.
 */
add_action('init', 'my_add_customfields_support_for_events');

function my_add_customfields_support_for_events() {
    add_post_type_support('event', 'custom-fields');
}  



/**
 * Hide the extra fields from event and post creation pages.
 */
// add_action( 'do_meta_boxes', 'my_remove_meta_boxes' );

// function my_remove_meta_boxes() {
// 	if (! current_user_can( 'manage_options' )) {
// 		remove_meta_box( 'postcustom', 'post', 'normal' );
//         remove_meta_box( 'postcustom', 'event', 'normal' );
// 	}
// }