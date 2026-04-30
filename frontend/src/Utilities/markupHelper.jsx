import React from 'react';
import { Link } from 'react-router-dom';

export const parseMarkup = (markup) => {
    if (!markup) return [];
    
    const parts = [];
    const linkRegex = /\[(.*?)\]\((.*?)\)/g;
    let lastIndex = 0;
    let match;
    
    while ((match = linkRegex.exec(markup)) !== null) {
        if (match.index > lastIndex) {
            parts.push(...parseBoldMarkup(markup.substring(lastIndex, match.index)));
        }
        parts.push({
            type: 'link',
            url: match[2],
            children: parseBoldMarkup(match[1])
        });
        lastIndex = match.index + match[0].length;
    }
    
    if (lastIndex < markup.length) {
        parts.push(...parseBoldMarkup(markup.substring(lastIndex)));
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
            parts.push({ type: 'text', content: text.substring(lastIndex, match.index) });
        }
        parts.push({ type: 'bold', content: match[1] });
        lastIndex = match.index + match[0].length;
    }
    
    if (lastIndex < text.length) {
        parts.push({ type: 'text', content: text.substring(lastIndex) });
    }
    return parts;
};

export const stripMarkup = (markup) => {
    if (!markup) return '';
    let text = markup.replace(/\[(.*?)\]\([^)]+\)/g, '$1');
    text = text.replace(/\*\*([^*]+)\*\*/g, '$1');
    return text;
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
