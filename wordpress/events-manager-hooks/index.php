<?php

/**
 * Plugin Name: Events manager hooks
 */

add_action('em_event_save', 'notify_event_saved', 10, 2);

function notify_event_saved ($false, $event) {
    $post = get_post($event->post_id);
    $created  = strtotime($post->post_date_gmt);
    $modified = strtotime($post->post_modified_gmt);

    if($created == $modified){
        $body = array(
            'event_id' => $event->event_id,
            'trigger' => 'create'
        );
        $args = array('body' => $body);
        //wp_remote_post('http://httpbin/post', $args);
        wp_remote_get('http://httpbin/get', $args);
    } 
    else {
        $body = array(
            'event_id' => $event->event_id,
            'trigger' => 'update'
        );
        $args = array('body' => $body);
        //wp_remote_post('http://httpbin/post', $args);
        wp_remote_get('http://httpbin/get', $args);
    }
}

add_action('em_booking_save', 'notify_booking_saved', 10, 2);

function notify_booking_saved ($count, $booking) {
    $body = array(
        'booking_id' => $booking->booking_id,
        'trigger' => 'create'
    );
    $args = array('body' => $body);
    //wp_remote_post('http://httpbin/post', $args);
    wp_remote_get('http://httpbin/get', $args);
}

// Create hook for users. Check event insert event.

// do_action('before_delete_post', 'notify_event_deleting', 10, 2);

// function notify_event_deleting ($postid, $post) {
//     $event = em_get_event($postid, 'post_id');
//     $body = array(
//         'event_id' => $event->event_id,
//         'trigger' => 'delete'
//     );
//     $args = array('body' => $body);
//     //wp_remote_post('http://httpbin/post', $args);
//     wp_remote_get('http://httpbin/get', $args);

//     return $result != false;
// }

// add_action('em_booking_deleted', 'notify_booking_deleted', 10, 1);

// function notify_booking_deleted ($booking) {
//     $body = array(
//         'booking_id' => $booking->booking_id,
//         'trigger' => 'delete'
//     );
//     $args = array('body' => $body);
//     //wp_remote_post('http://httpbin/post', $args);
//     wp_remote_get('http://httpbin/get', $args);
// }

 // add_action('save_post_event', 'notify_event_create', 10, 3);

// function notify_event_create ($post_ID, $post, $update) {

//     // if ($post->post_type != 'event') {
//     //     return;
//     // }
    
//     if ($update) {
//         return;
//     }

//     $EM_Event = new EM_Event($post_ID, 'post_id');
//     $body = array(
//         'event_id' => $EM_Event->event_id
//     );
//     $args = array('body' => $body);
//     wp_remote_post('http://httpbin/post', $args);
// }

// add_action('publish_event', 'on_event_initial_publish', 10, 2);

// function on_event_initial_publish($post_ID, $post) {

//     $created  = strtotime($post->post_date_gmt);
//     $modified = strtotime($post->post_modified_gmt);

//     if($created == $modified){
//         $EM_Event = em_get_event($post_ID, 'post_id');
//         $body = array(
//             'event_id' => $EM_Event->output("#_EVENTID"),
//             // 'event_id' => $EM_Event->output("#_EVENTID"),
//             //'trigger' => 'create'
//         );
//         $args = array('body' => $body);
//         //wp_remote_post('http://httpbin/post', $args);
//         wp_remote_get('http://httpbin/get', $args);
//     } 
//     else {
//         $EM_Event = em_get_event($post_ID, 'post_id');
//         $body = array(
//             'event_id' => $EM_Event->event_id,
//             //'trigger' => 'update'
//         );
//         $args = array('body' => $body);
//         //wp_remote_post('http://httpbin/post', $args);
//         wp_remote_get('http://httpbin/get', $args);
//     }
// }

