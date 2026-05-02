import React from 'react';
import { Link } from 'react-router-dom';

const ESCAPES = {
    '\\\\': '___ESC_BS___',
    '\\*': '___ESC_AST___',
    '\\[': '___ESC_LB___',
    '\\]': '___ESC_RB___',
    '\\(': '___ESC_LP___',
    '\\)': '___ESC_RP___'
};

const applyEscapes = (text) => {
    let result = text;
    for (const [key, value] of Object.entries(ESCAPES)) {
        result = result.split(key).join(value);
    }
    return result;
};

const restoreEscapes = (text) => {
    let result = text;
    for (const [key, value] of Object.entries(ESCAPES)) {
        result = result.split(value).join(key.substring(1));
    }
    return result;
};

export const parseMarkup = (markup) => {
    if (!markup) return [];
    
    const escapedMarkup = applyEscapes(markup);
    const parts = [];
    const linkRegex = /\[(.*?)\]\((.*?)\)/g;
    let lastIndex = 0;
    let match;
    
    while ((match = linkRegex.exec(escapedMarkup)) !== null) {
        if (match.index > lastIndex) {
            parts.push(...parseBoldMarkup(escapedMarkup.substring(lastIndex, match.index)));
        }
        parts.push({
            type: 'link',
            url: restoreEscapes(match[2]),
            children: parseBoldMarkup(match[1])
        });
        lastIndex = match.index + match[0].length;
    }
    
    if (lastIndex < escapedMarkup.length) {
        parts.push(...parseBoldMarkup(escapedMarkup.substring(lastIndex)));
    }
    return parts;
};

const parseBoldMarkup = (text) => {
    const parts = [];
    const boldRegex = /\*\*([^*]+)\*\*/g;
    let lastIndex = 0;
    let match;
    
    while ((match = boldRegex.exec(text)) !== null) {
        if (match.index > lastIndex) {
            parts.push({ type: 'text', content: restoreEscapes(text.substring(lastIndex, match.index)) });
        }
        parts.push({ type: 'bold', content: restoreEscapes(match[1]) });
        lastIndex = match.index + match[0].length;
    }
    
    if (lastIndex < text.length) {
        parts.push({ type: 'text', content: restoreEscapes(text.substring(lastIndex)) });
    }
    return parts;
};

export const stripMarkup = (markup) => {
    if (!markup) return '';
    const escaped = applyEscapes(markup);
    let text = escaped.replace(/\[(.*?)\]\([^)]+\)/g, '$1');
    text = text.replace(/\*\*([^*]+)\*\*/g, '$1');
    return restoreEscapes(text);
};

export const renderAst = (ast, keyPrefix = '') => {
    return ast.map((node, i) => {
        const key = `${keyPrefix}-${i}`;
        if (node.type === 'text') {
            return <React.Fragment key={key}>{node.content}</React.Fragment>;
        }
        if (node.type === 'bold') {
            return <strong key={key}>{node.content}</strong>;
        }
        if (node.type === 'link') {
            return (
                <Link key={key} to={node.url} className="log-link target-link" onClick={e => e.stopPropagation()}>
                    {renderAst(node.children, key)}
                </Link>
            );
        }
        return null;
    });
};
