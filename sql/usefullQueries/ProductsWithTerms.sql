SELECT p.ID, p.post_title, p.post_status FROM wptj_posts p
JOIN wptj_term_relationships tr ON  tr.object_id = p.ID
JOIN wptj_term_taxonomy tt ON tt.term_taxonomy_id = tr.term_taxonomy_id
JOIN wptj_terms t ON t.term_id = tt.term_id
WHERE
p.post_type = 'product' AND
tt.taxonomy = 'pa_fsize' AND
t.name = '06 (xs)';