<?php

/**
 * Plugin Name: Events manager hooks
 */

add_filter('em_event_save', 'notify_event', 10, 2);

function notify_event($result, $event) {
    $post = get_post($event->post_id);
    $created  = strtotime($post->post_date_gmt);
    $modified = strtotime($post->post_modified_gmt);
    $event_start = strtotime($event->event_start_date . " " . $event->event_start_time);
    $event_end = strtotime($event->event_start_date . " " . $event->event_start_time);
    // Event created.
    if ($created == $modified && $event->event_status == 1) {
        $status = true;
        $method = 'CREATE';
        send_event_payload($event, $attributes = [], $event_start, $event_end, $status, $method);
    } 
    // Event placed in draft.
    // else if (is_null($event->event_status)) {
    //     $status = false;
    //     $method = 'UPDATE';
    //     send_event_payload($event, $attributes = [], $event_start, $event_end, $status, $method);
    // }

    return $result;
}

// Event updated. Only modified attributes are sent to Producer.
// add_action('em_event_save_pre', 'notify_event_update');

// function notify_event_update($event) {
//     $post = get_post($event->post_id);
//     $created  = strtotime($post->post_date_gmt);
//     $modified = strtotime($post->post_modified_gmt);
//     $event_start = strtotime($event->event_start_date . " " . $event->event_start_time);
//     $event_end = strtotime($event->event_start_date . " " . $event->event_start_time);
    
//     if ($created != $modified && $event->event_status == 1) {
//         $event_id = $event->event_id;
//         global $wpdb;
//         $old = $wpdb->get_row($wpdb->prepare("SELECT * FROM " . EM_EVENTS_TABLE . " WHERE event_id=%s", $event_id), ARRAY_A);
//         $master = array('event_id' => 0, 'event_owner' => 0, 'event_status' => 0, 'event_name' => 0, 'event_start' => 0, 'event_end' => 0);
//         $old_intersect = array_intersect_key($old, $master);
//         $old_intersect["event_start"] = strtotime($old_intersect["event_start"]);
//         $old_intersect["event_end"] = strtotime($old_intersect["event_end"]);
//         $event_array = json_decode(json_encode($event), true); // Convert to array.
//         $event_array_intersect = array_intersect_key($event_array, $master);
//         $event_array_intersect["event_start"] = strtotime($event->event_start_date." ".$event->event_start_time);
//         $event_array_intersect["event_end"] = strtotime($event->event_end_date." ".$event->event_end_time);
//         $diff = array_diff($old_intersect, $event_array_intersect);
//         if (!empty($diff)) {
//             $status = true;
//             $method = 'UPDATE';
//             $attributes = array();
//             foreach (array_keys($diff) as $key) {
//                 $attributes[$key] = $event_array_intersect[$key];
//             };
//             send_event_payload($event, $attributes, $event_start, $event_end, $status, $method);
//         }
//     } 
// }

// Event placed in trash.
// add_action('wp_trash_post', 'notify_event_delete');

// function notify_event_delete($post_id) {
//     global $wpdb;
//     $event = $wpdb->get_row($wpdb->prepare("SELECT * FROM " . EM_EVENTS_TABLE . " WHERE post_id=%s", $post_id), OBJECT);
//     $headers = array(
//         'Content-Type' => 'application/json'
//     );
//     $args = array(
//         'method' => 'DELETE',
//         'headers' => $headers     
//     );
//     wp_remote_request("http://producer:5000/api/events/{$event->id}", $args);
// }

function send_event_payload($event, $attributes, $event_start, $event_end, $status, $method) {
    $headers = array(
        'Content-Type' => 'application/json'
    );

    if ($method == 'CREATE') {
        $body = array(
            'id' => $event->event_id,
            'owner' => $event->event_owner, // OrganiserUuid
            'status' => $status, // IsActive
            'name' => $event->event_name, // Title
            'start' => $event_start, // StartDateUTC
            'end' => $event_end, // EndDateUTC
        );

        $body = wp_json_encode($body);

        $args = array(
            'headers' => $headers,     
            'body' => $body,
            'data_format' => 'body'
        );

        wp_remote_post("http://producer:5000/api/events", $args);
    } else if ($method == 'UPDATE') {
        $attributes = array_combine(
            array_map(
                function($subject) {
                    return str_replace("event_", "", $subject);
                }, array_keys($attributes)
            ), array_values($attributes)
        );
        $attributes + array(
            'id' => $event->event_id,
            'owner' => $event->owner
        ); // Business rule: event owner should be allowed to change. Not enforceable in settings.
        $body = wp_json_encode($attributes);

        $args = array(
            'method' => 'PATCH',
            'headers' => $headers,     
            'body' => $body,
            'data_format' => 'body'
        );
        
        wp_remote_request("http://producer:5000/api/events/{$event->event_id}", $args);
    }
}

// 0 => 'Pending',
// 1 => Approved',
// 2 => 'Rejected',
// 3 => 'Cancelled',
// 4 => 'Awaiting Online Payment',
// 5 => 'Awaiting Payment'
// Booking created.
add_filter('em_booking_save', 'notify_booking_created', 10, 3);

function notify_booking_created ($count, $booking, $update) {
    $method = 'CREATE';
    $attributes = $booking->to_array();
    $first_name = get_user_meta($booking->person_id, 'first_name', true);
    $last_name = get_user_meta($booking->person_id, 'last_name', true);
    $user_data = get_userdata($booking->person_id);
    $attributes['first_name'] = $first_name;
    $attributes['last_name'] = $last_name;
    $attributes['email'] = $user_data->user_email;;
    
    send_booking_payload($booking->id, $attributes, $method);

    return $count;
}

// Booking status updated.
// add_filter('em_booking_set_status','notify_booking_status', 10, 2);

// function notify_booking_status($result, $booking) {
//     $method = 'UPDATE';
//     $attributes = array('bookingStatus' => $booking->booking_status);
//     $meta = get_user_meta($booking->person_id);
//     $first_name = $meta['first_name'][0];
//     $last_name = $meta['last_name'][0];
//     $user_data = get_userdata($booking->person_id);
//     $email = $user_data->user_email;
//     $attributes + array (
//         'first_name' => $first_name,
//         'last_name' => $last_name,
//         'email' => $email
//     );
//     send_booking_payload($booking->id, $attributes, $method);
//     return $result;
// }

// Booking deleted.
// add_filter('em_booking_delete', 'notify_booking_deleted', 10, 2);

// function notify_booking_deleted ($result, $booking) {
//     $headers = array(
//         'Content-Type' => 'application/json'
//     );
//     $args = array(
//         'method' => 'DELETE',
//         'headers' => $headers     
//     );
//     wp_remote_request("http://producer:5000/api/bookings/{$booking->id}", $args);

//     return $result;
// }

function send_booking_payload($booking_id, $attributes, $method) {
    $headers = array(
        'Content-Type' => 'application/json'
    );
    
    if ($method == 'CREATE') {
        $body = array(
            'id' => $attributes['booking_id'],
            'eventId' => $attributes['event_id'],
            'personId' => $attributes['person_id'], // Name, LastName, Email, VatNumber
            'bookingSpaces' => $attributes['booking_spaces'],
            'bookingStatus' => $attributes['booking_status'], // InvitationStatus
            'firstName' => $attributes['first_name'],
            'lastName' => $attributes['last_name'],
            'email' => $attributes['email']
        );  

        $body = wp_json_encode($body);

        $args = array(
            'headers' => $headers,     
            'body' => $body
        );

        wp_remote_post("http://producer:5000/api/bookings", $args);
    } else if ($method == 'UPDATE') {
        $body = wp_json_encode($attributes);

        $args = array(
            'method' => 'PATCH',
            'headers' => $headers,     
            'body' => $body,
            'data_format' => 'body'
        );
        
        wp_remote_request("http://producer:5000/api/bookings/{$booking_id}", $args);
    }
}